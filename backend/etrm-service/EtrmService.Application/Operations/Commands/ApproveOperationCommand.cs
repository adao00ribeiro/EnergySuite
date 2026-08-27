using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.ImerisIntegration;

namespace EtrmService.Application.Operations.Commands;

public class ApproveOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApproveOperationCommand : IRequest<ApproveOperationResponse>
{
    public Guid OpportunityId { get; set; }
    // No MVP, mockamos o volume da operação com base no que foi simulado.
    public decimal RequestedVolumeMwm { get; set; }
}

public class ApproveOperationCommandHandler : IRequestHandler<ApproveOperationCommand, ApproveOperationResponse>
{
    private readonly IImerisCreditClient _imerisClient;

    public ApproveOperationCommandHandler(IImerisCreditClient imerisClient)
    {
        _imerisClient = imerisClient;
    }

    public async Task<ApproveOperationResponse> Handle(ApproveOperationCommand request, CancellationToken cancellationToken)
    {
        // Simulando que a contraparte está fixada no contexto da oportunidade
        var mockCounterpartyId = Guid.NewGuid();

        var validationResult = await _imerisClient.ValidateLimitAsync(mockCounterpartyId, request.RequestedVolumeMwm);

        if (!validationResult.IsApproved)
        {
            return new ApproveOperationResponse
            {
                Success = false,
                Message = $"Risco de Crédito Reprovado (Imeris): {validationResult.Reason}"
            };
        }

        // Aqui, a operação seria persistida no banco e enviada para o BackOps...
        
        return new ApproveOperationResponse
        {
            Success = true,
            Message = "Operação aprovada com sucesso. O BackOps foi notificado."
        };
    }
}
