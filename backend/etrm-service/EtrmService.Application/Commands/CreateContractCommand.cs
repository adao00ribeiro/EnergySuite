using System;
using MediatR;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Commands;

public class CreateContractCommand : IRequest<Guid>
{
    public string CounterpartyName { get; set; } = string.Empty;
    public ContractType Type { get; set; }
    public EnergySubmarket Submarket { get; set; }
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal? StrikePrice { get; set; }
    public decimal? OptionPremium { get; set; }
}
