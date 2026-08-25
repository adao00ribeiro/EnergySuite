using System;

namespace EtrmService.Application.IntegrationEvents
{
    public record RiskCalculatedIntegrationEvent(
        Guid ContractId,
        string CounterpartyName,
        decimal FinancialExposure,
        string RiskCategory,
        DateTime CalculatedAt
    );
}
