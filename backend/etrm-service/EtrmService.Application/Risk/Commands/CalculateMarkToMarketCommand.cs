using System;
using System.Collections.Generic;
using MediatR;

namespace EtrmService.Application.Risk.Commands
{
    public class ContractPositionDto
    {
        public Guid ContractId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Submarket { get; set; } = string.Empty;
        public decimal VolumeMWm { get; set; }
        public decimal ContractPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; } = "BUY"; // BUY / SELL
    }

    public class MarkToMarketResultDto
    {
        public decimal TotalMtMValue { get; set; }
        public decimal TotalExposureValue { get; set; }
        public string Currency { get; set; } = "BRL";
        public List<ContractMtMDetailDto> ContractDetails { get; set; } = new();
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ContractMtMDetailDto
    {
        public Guid ContractId { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal VolumeMWm { get; set; }
        public decimal ContractPrice { get; set; }
        public decimal CurrentMarketPrice { get; set; }
        public decimal MtMValue { get; set; }
        public decimal MtMPercent { get; set; }
    }

    public class CalculateMarkToMarketCommand : IRequest<MarkToMarketResultDto>
    {
        public Guid? PortfolioId { get; set; }
        public List<ContractPositionDto> Positions { get; set; } = new();
    }
}
