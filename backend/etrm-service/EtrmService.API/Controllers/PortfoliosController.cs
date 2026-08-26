using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Portfolios.Commands;
using EtrmService.Application.Portfolios.Queries;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class PortfoliosController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PortfoliosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePortfolioCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetPortfoliosQuery());
        return Ok(result);
    }
}
