using System;
using MediatR;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Operations.Commands;

public class CreateOperationCommand : IRequest<Guid>
{
    public Guid TicketId { get; set; }
    public Guid PortfolioId { get; set; }
    public Guid CounterpartyId { get; set; }
    public OperationType Type { get; set; }
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
