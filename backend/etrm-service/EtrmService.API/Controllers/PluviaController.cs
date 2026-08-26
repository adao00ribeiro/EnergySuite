using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Pluvia.Commands;
using EtrmService.Application.Pluvia.DTOs;
using EtrmService.Application.Pluvia.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class PluviaController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public PluviaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("scenarios")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateScenario([FromBody] CreatePrecipitationScenarioCommand command)
    {
        var scenarioId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetScenarios), new { id = scenarioId }, scenarioId);
    }

    [HttpGet("metadata")]
    public async Task<IActionResult> GetForecastMetadata()
    {
        var query = new GetForecastMetadataQuery();
        var result = await _mediator.Send(query);
        
        // Mock fallback if db is empty for UI testing
        if (!result.Any())
        {
            var mockData = new List<ForecastMetadataDto>
            {
                new ForecastMetadataDto { Id = Guid.NewGuid(), ModelName = "GEFS", ReferenceDate = DateTime.UtcNow.Date, Resolution = "0p50", EnsembleMembers = 30, LakehousePath = "s3://datalake/bronze/meteorology/gefs", CreatedAt = DateTime.UtcNow },
                new ForecastMetadataDto { Id = Guid.NewGuid(), ModelName = "ECMWF", ReferenceDate = DateTime.UtcNow.Date, Resolution = "0p25", EnsembleMembers = 1, LakehousePath = "s3://datalake/bronze/meteorology/ecmwf", CreatedAt = DateTime.UtcNow },
                new ForecastMetadataDto { Id = Guid.NewGuid(), ModelName = "ETA", ReferenceDate = DateTime.UtcNow.Date, Resolution = "15km", EnsembleMembers = 1, LakehousePath = "s3://datalake/bronze/meteorology/eta", CreatedAt = DateTime.UtcNow }
            };
            return Ok(mockData);
        }

        return Ok(result);
    }

    [HttpGet("scenarios")]
    [ProducesResponseType(typeof(IEnumerable<PrecipitationScenarioDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScenarios()
    {
        var query = new GetPrecipitationScenariosQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
