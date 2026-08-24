using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using EtrmService.Application.Commands;

namespace EtrmService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContractsController : ControllerBase
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
}
