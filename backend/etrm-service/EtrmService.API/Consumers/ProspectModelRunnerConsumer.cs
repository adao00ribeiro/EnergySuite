using System;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.Events;
using EtrmService.API.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class ProspectModelRunnerConsumer : IConsumer<StudyExecutionRequestedEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly IHubContext<ProspectHub> _hubContext;
    private readonly ILogger<ProspectModelRunnerConsumer> _logger;

    public ProspectModelRunnerConsumer(IEtrmDbContext context, IHubContext<ProspectHub> hubContext, ILogger<ProspectModelRunnerConsumer> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StudyExecutionRequestedEvent> context)
    {
        var studyId = context.Message.StudyId;
        var group = _hubContext.Clients.Group($"Study_{studyId}");

        _logger.LogInformation("Iniciando execução do Estudo {StudyId}", studyId);
        await group.SendAsync("LogReceived", $"[WORKER] Iniciando processamento do Estudo {studyId}...");

        var study = await _context.ProspectStudies.FirstOrDefaultAsync(s => s.Id == studyId);
        if (study == null)
        {
            await group.SendAsync("LogReceived", $"[ERROR] Estudo {studyId} não encontrado no banco.");
            return;
        }

        study.ChangeState(Domain.Enums.StudyState.Running);
        await _context.SaveChangesAsync(context.CancellationToken);
        await group.SendAsync("LogReceived", $"[SYSTEM] Status do Estudo alterado para RUNNING.");

        var decks = await _context.ProspectDecks.Where(d => d.StudyId == studyId).OrderBy(d => d.SequenceOrder).ToListAsync(context.CancellationToken);
        
        foreach(var deck in decks)
        {
            await group.SendAsync("LogReceived", $"[WORKER] Preparando Deck Mês {deck.SequenceOrder} ({deck.Period:MM/yyyy})...");
            deck.ChangeState(Domain.Enums.DeckState.Running);
            await _context.SaveChangesAsync(context.CancellationToken);

            // Simulate heavy computation (e.g. running GEVAZP/NEWAVE)
            await Task.Delay(2000, context.CancellationToken); 

            await group.SendAsync("LogReceived", $"[WORKER] Simulação do Deck {deck.SequenceOrder} concluída com sucesso.");
            deck.ChangeState(Domain.Enums.DeckState.Completed);
            await _context.SaveChangesAsync(context.CancellationToken);
        }

        study.ChangeState(Domain.Enums.StudyState.Completed);
        await _context.SaveChangesAsync(context.CancellationToken);

        await group.SendAsync("LogReceived", $"[SYSTEM] Execução finalizada! Status do Estudo: COMPLETED.");
        _logger.LogInformation("Execução do Estudo {StudyId} finalizada.", studyId);
    }
}
