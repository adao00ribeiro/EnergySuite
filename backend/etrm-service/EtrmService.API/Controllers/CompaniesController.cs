using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.CommercialRegistry.Commands;
using EtrmService.Application.CommercialRegistry.Queries;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class CompaniesController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetCompaniesQuery());
        return Ok(result);
    }
}
