using System;
using System.Collections.Generic;

namespace EtrmService.Application.Finance.DTOs;

public class OpenSettlementDto
{
    public Guid Id { get; set; }
    public Guid CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = string.Empty;
    public string ReferenceMonth { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class OperationToBillDto
{
    public Guid Id { get; set; }
    public Guid CounterpartyId { get; set; }
    public string CounterpartyName { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class FinanceTotalsDto
{
    public decimal TotalPayable { get; set; }
    public decimal TotalReceivable { get; set; }
    public decimal NetBalance { get; set; }
}

public class FinancialDashboardDto
{
    public List<OpenSettlementDto> OpenSettlements { get; set; } = new();
    public List<OperationToBillDto> OperationsToBill { get; set; } = new();
    public FinanceTotalsDto Totals { get; set; } = new();
}
