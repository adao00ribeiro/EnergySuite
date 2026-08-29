using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.ImerisIntegration;
using EtrmService.Application.Operations.Services;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Operations.Commands;

public class ApproveOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApproveOperationCommand : IRequest<ApproveOperationResponse>
{
    public Guid OperationId { get; set; }
    public Guid OpportunityId { get; set; }
    public decimal RequestedVolumeMwm { get; set; }
}

public class ApproveOperationCommandHandler : IRequestHandler<ApproveOperationCommand, ApproveOperationResponse>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImerisCreditClient _imerisClient;
    private readonly IWebhookNotifierService _webhookNotifier;

    public ApproveOperationCommandHandler(
        IEtrmDbContext context,
        ICurrentUserService currentUserService,
        IImerisCreditClient imerisClient,
        IWebhookNotifierService webhookNotifier)
    {
        _context = context;
        _currentUserService = currentUserService;
        _imerisClient = imerisClient;
        _webhookNotifier = webhookNotifier;
    }

    public async Task<ApproveOperationResponse> Handle(ApproveOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);

        if (operation == null)
        {
            return new ApproveOperationResponse
            {
                Success = false,
                Message = "Operação não encontrada."
            };
        }

        var counterpartyId = operation.CounterpartyId;
        var operationVolume = request.RequestedVolumeMwm > 0m ? request.RequestedVolumeMwm : operation.VolumeMwMed;

        var validationResult = await _imerisClient.ValidateLimitAsync(counterpartyId, operationVolume);

        if (!validationResult.IsApproved)
        {
            await _webhookNotifier.NotifyRiskViolationAsync(request.OpportunityId != Guid.Empty ? request.OpportunityId : operation.Id, validationResult.Reason);

            return new ApproveOperationResponse
            {
                Success = false,
                Message = $"Risco de Crédito Reprovado (Imeris): {validationResult.Reason}"
            };
        }

        var oldState = operation.State.ToString();
        operation.ChangeState(OperationState.Approved);

        var changes = JsonSerializer.Serialize(new { OldState = oldState, NewState = OperationState.Approved.ToString(), CounterpartyId = counterpartyId });
        var auditLog = new AuditLog("Operation", operation.Id.ToString(), "Approved", changes, _currentUserService.UserId ?? "system", _currentUserService.TenantId);

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);

        await _webhookNotifier.NotifyApprovedAsync(operation.Id, $"Operação aprovada com {operationVolume} MWm.");

        return new ApproveOperationResponse
        {
            Success = true,
            Message = "Operação aprovada com sucesso. O BackOps foi notificado."
        };
    }
}
