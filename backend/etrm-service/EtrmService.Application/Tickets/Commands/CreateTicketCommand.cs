using System;
using MediatR;

namespace EtrmService.Application.Tickets.Commands;

public class CreateTicketCommand : IRequest<Guid>
{
    public string ReferenceNumber { get; set; } = string.Empty;
}
