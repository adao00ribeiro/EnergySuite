using System;

namespace EtrmService.Application.Operations.DTOs;

public class OperationDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid CounterpartyId { get; set; }
    public string TicketRef { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
