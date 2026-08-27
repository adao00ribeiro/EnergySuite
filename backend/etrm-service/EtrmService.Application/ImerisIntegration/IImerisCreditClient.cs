using System;
using System.Threading.Tasks;

namespace EtrmService.Application.ImerisIntegration;

public class CreditValidationResult
{
    public bool IsApproved { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public interface IImerisCreditClient
{
    Task<CreditValidationResult> ValidateLimitAsync(Guid counterpartyId, decimal operationVolumeMwm);
}
