using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Pricing.DTOs;
using EtrmService.Application.Pricing.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class PricingController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PricingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("forward-curve")]
    [ProducesResponseType(typeof(IEnumerable<ForwardCurvePointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForwardCurve()
    {
        var result = await _mediator.Send(new GetForwardCurveQuery());
        return Ok(result);
    }
}
