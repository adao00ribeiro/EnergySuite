using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Operations.Commands;
using EtrmService.Application.Operations.Queries;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class OperationsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public OperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOperationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetOperationsQuery());
        return Ok(result);
    }
    
    [HttpPatch("{id}/state")]
    public async Task<IActionResult> ChangeState(System.Guid id, [FromBody] ChangeOperationStateCommand command)
    {
        command.OperationId = id;
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        
        return NoContent();
    }
}
