using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using EtrmService.Application.Commands;
using EtrmService.Application.Queries;

using EtrmService.API.Controllers.Shared;
using Asp.Versioning;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class ContractsController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractCommand command)
    {
        var contractId = await _mediator.Send(command);
        return Ok(new { Id = contractId });
    }

    [HttpGet]
    public async Task<IActionResult> GetContracts()
    {
        var query = new GetContractsListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContractById(Guid id)
    {
        var query = new GetContractByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
