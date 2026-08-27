using System;
using System.Threading.Tasks;

namespace EtrmService.Application.ImerisIntegration;

public class ImerisCreditClient : IImerisCreditClient
{
    public async Task<CreditValidationResult> ValidateLimitAsync(Guid counterpartyId, decimal operationVolumeMwm)
    {
        await Task.Delay(200); // Simulate network call to Imeris API

        // Heurística de Mock: Reprova operações acima de 20 MWm para testar a trava de crédito
        if (operationVolumeMwm > 20m)
        {
            return new CreditValidationResult
            {
                IsApproved = false,
                Reason = $"Limite de Crédito Excedido para a Contraparte. O volume de {operationVolumeMwm} MWm ultrapassa o limite pré-aprovado de 20 MWm."
            };
        }

        return new CreditValidationResult
        {
            IsApproved = true,
            Reason = "Crédito Aprovado."
        };
    }
}
