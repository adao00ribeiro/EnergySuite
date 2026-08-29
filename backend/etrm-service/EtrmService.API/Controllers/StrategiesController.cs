using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Strategies.Commands;
using EtrmService.Application.Strategies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class StrategiesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public StrategiesController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetStrategiesQuery { TenantId = _currentUser.TenantId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStrategyCommand command)
    {
        command.TenantId = _currentUser.TenantId;
        var result = await _mediator.Send(command);
        return Ok(new { id = result });
    }
}