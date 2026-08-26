using System.Collections.Generic;
using MediatR;
using EtrmService.Application.Tickets.DTOs;

namespace EtrmService.Application.Tickets.Queries;

public class GetTicketsQuery : IRequest<List<TicketDto>>
{
}
