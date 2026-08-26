using MediatR;
using System;

namespace EtrmService.Application.Operations.Commands;

public record CreateSwapOperationCommand(
    Guid TicketId,
    Guid PortfolioId,
    Guid CounterpartyId,
    decimal VolumeMwMed,
    decimal Price,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<(Guid LegAId, Guid LegBId)>;
