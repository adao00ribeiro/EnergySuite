using System;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Events;
using EtrmService.Application.Prospect.Services;
using EtrmService.Domain.Interfaces;
using EtrmService.API.Hubs;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Consumers;

public class ProspectModelRunnerConsumer : IConsumer<StudyExecutionRequestedEvent>
{
    private readonly IProspectRepository _prospectRepository;
    private readonly IHubContext<ProspectHub> _hubContext;
    private readonly ILogger<ProspectModelRunnerConsumer> _logger;
    private readonly IWebhookService _webhookService;

    public ProspectModelRunnerConsumer(IProspectRepository prospectRepository, IHubContext<ProspectHub> hubContext, ILogger<ProspectModelRunnerConsumer> logger, IWebhookService webhookService)
    {
        _prospectRepository = prospectRepository;
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

        var study = await _prospectRepository.GetStudyByIdAsync(studyId, context.CancellationToken);
        if (study == null)
        {
            await group.SendAsync("LogReceived", $"[ERROR] Estudo {studyId} não encontrado no banco.");
            return;
        }

        study.ChangeState(Domain.Enums.StudyState.Running);
        await _prospectRepository.SaveChangesAsync(context.CancellationToken);
        await group.SendAsync("LogReceived", $"[SYSTEM] Status do Estudo alterado para RUNNING.");
        await _webhookService.SendWebhookAsync("study.started", new { StudyId = study.Id, Status = "Running" });

        var decks = await _prospectRepository.GetDecksByStudyIdAsync(studyId, context.CancellationToken);
        
        foreach(var deck in decks)
        {
            await group.SendAsync("LogReceived", $"[WORKER] Preparando Deck Mês {deck.SequenceOrder} ({deck.Period:MM/yyyy})...");
            deck.ChangeState(Domain.Enums.DeckState.Running);
            await _prospectRepository.SaveChangesAsync(context.CancellationToken);

            await group.SendAsync("LogReceived", $"[WORKER] Simulação do Deck {deck.SequenceOrder} concluída com sucesso.");
            deck.ChangeState(Domain.Enums.DeckState.Completed);
            await _prospectRepository.SaveChangesAsync(context.CancellationToken);
            await _webhookService.SendWebhookAsync("deck.completed", new { DeckId = deck.Id, StudyId = study.Id, Sequence = deck.SequenceOrder });
        }

        study.ChangeState(Domain.Enums.StudyState.Completed);
        study.SaveResults("[]");
        await _prospectRepository.SaveChangesAsync(context.CancellationToken);

        await group.SendAsync("LogReceived", $"[SYSTEM] Execução finalizada! Status do Estudo: COMPLETED.");
        _logger.LogInformation("Execução do Estudo {StudyId} finalizada.", studyId);
        await _webhookService.SendWebhookAsync("study.completed", new { StudyId = study.Id, Status = "Completed" });
    }
}

