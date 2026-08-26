using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Tickets.Commands;
using EtrmService.Application.Tickets.Queries;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class TicketsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public TicketsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTicketCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetTicketsQuery());
        return Ok(result);
    }
}
