using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.IntegrationEvents
{
    public record ContractCreatedIntegrationEvent(
        Guid ContractId,
        string CounterpartyName,
        ContractType Type,
        EnergySubmarket Submarket,
        decimal VolumeMwMed,
        decimal Price,
        DateTime StartDate,
        DateTime EndDate,
        DateTime CreatedAt,
        decimal? StrikePrice = null,
        decimal? OptionPremium = null,
        Guid TenantId = default
    );
}
