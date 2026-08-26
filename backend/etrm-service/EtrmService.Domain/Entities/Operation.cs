using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class Operation
{
    public Guid Id { get; private set; }
    public Guid TicketId { get; private set; }
    public Guid PortfolioId { get; private set; }
    public Guid CounterpartyId { get; private set; }
    
    public OperationType Type { get; private set; }
    public OperationState State { get; private set; }
    
    public decimal VolumeMwMed { get; private set; }
    public decimal Price { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    public Guid TenantId { get; private set; }
    
    // Navigation properties
    public Ticket Ticket { get; private set; } = null!;
    public Portfolio Portfolio { get; private set; } = null!;
    public Company Counterparty { get; private set; } = null!;

    protected Operation() { }

    public Operation(Guid ticketId, Guid portfolioId, Guid counterpartyId, OperationType type, decimal volumeMwMed, decimal price, DateTime startDate, DateTime endDate, Guid tenantId)
    {
        Id = Guid.NewGuid();
        TicketId = ticketId;
        PortfolioId = portfolioId;
        CounterpartyId = counterpartyId;
        Type = type;
        State = OperationState.Draft;
        VolumeMwMed = volumeMwMed;
        Price = price;
        StartDate = startDate;
        EndDate = endDate;
        TenantId = tenantId;
    }

    public void ChangeState(OperationState newState)
    {
        State = newState;
    }
}
