using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.Application.Pluvia.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/b2b/pluvia")]
[ApiController]
[Authorize(Policy = "EnaPolicy")]
[EnableRateLimiting("b2b")]
public class B2bPluviaController : ControllerBase
{
    private readonly IMediator _mediator;

    public B2bPluviaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retorna as projeções mais recentes de Energia Natural Afluente (ENA)
    /// </summary>
    [HttpGet("ena")]
    [ProducesResponseType(typeof(IEnumerable<EnaResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> GetEnaResults([FromQuery] string? submarket)
    {
        var query = new GetEnaResultsQuery 
        { 
            Submarket = submarket,
            OffsetDays = 0 
        };
        
        var results = await _mediator.Send(query);
        return Ok(results);
    }
}
