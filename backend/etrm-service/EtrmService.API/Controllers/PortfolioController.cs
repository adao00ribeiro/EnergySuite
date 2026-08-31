using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Operations.Commands;
using EtrmService.Application.Portfolios.Queries;
using EtrmService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class PortfolioController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PortfolioController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet("position")]
    public async Task<IActionResult> GetPosition([FromQuery] Guid? portfolioId, [FromQuery] int? year)
    {
        var resolvedPortfolioId = await _mediator.Send(new GetDefaultPortfolioIdQuery(portfolioId, _currentUser.TenantId));

        var query = new GetPortfolioPositionQuery(
            resolvedPortfolioId,
            _currentUser.TenantId,
            year ?? DateTime.UtcNow.Year);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("opportunities")]
    public async Task<IActionResult> GetOpportunities([FromQuery] Guid? portfolioId)
    {
        var query = new GetRankedOpportunitiesQuery
        {
            PortfolioId = portfolioId ?? Guid.Empty
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateOperationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] ApproveOperationCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}