using System;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.Events;
using EtrmService.Application.Prospect.Services;
using EtrmService.API.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EtrmService.API.Consumers;

public class ProspectModelRunnerConsumer : IConsumer<StudyExecutionRequestedEvent>
{
    private readonly IEtrmDbContext _context;
    private readonly IHubContext<ProspectHub> _hubContext;
    private readonly ILogger<ProspectModelRunnerConsumer> _logger;
    private readonly IWebhookService _webhookService;

    public ProspectModelRunnerConsumer(IEtrmDbContext context, IHubContext<ProspectHub> hubContext, ILogger<ProspectModelRunnerConsumer> logger, IWebhookService webhookService)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
        _webhookService = webhookService;
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
        await _webhookService.SendWebhookAsync("study.started", new { StudyId = study.Id, Status = "Running" });

        var decks = await _context.ProspectDecks
            .Include(d => d.Versions)
            .Where(d => d.StudyId == studyId)
            .OrderBy(d => d.SequenceOrder)
            .ToListAsync(context.CancellationToken);
        
        foreach(var deck in decks)
        {
            await group.SendAsync("LogReceived", $"[WORKER] Preparando Deck Mês {deck.SequenceOrder} ({deck.Period:MM/yyyy})...");
            deck.ChangeState(Domain.Enums.DeckState.Running);
            await _context.SaveChangesAsync(context.CancellationToken);

            await group.SendAsync("LogReceived", $"[WORKER] Simulação do Deck {deck.SequenceOrder} concluída com sucesso.");
            deck.ChangeState(Domain.Enums.DeckState.Completed);
            await _context.SaveChangesAsync(context.CancellationToken);
            await _webhookService.SendWebhookAsync("deck.completed", new { DeckId = deck.Id, StudyId = study.Id, Sequence = deck.SequenceOrder });
        }

        study.ChangeState(Domain.Enums.StudyState.Completed);
        study.SaveResults("[]");
        await _context.SaveChangesAsync(context.CancellationToken);

        await group.SendAsync("LogReceived", $"[SYSTEM] Execução finalizada! Status do Estudo: COMPLETED.");
        _logger.LogInformation("Execução do Estudo {StudyId} finalizada.", studyId);
        await _webhookService.SendWebhookAsync("study.completed", new { StudyId = study.Id, Status = "Completed" });
    }
}
