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

    [HttpPost("simulate")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status202Accepted)]
    public async Task<IActionResult> Simulate([FromBody] RunHydrologicalSimulationCommand command)
    {
        var executionId = await _mediator.Send(command);
        return Accepted(new { executionId });
    }

    [HttpPost("custom-maps/upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadCustomMap([FromForm] IFormFile file, [FromForm] string name, [FromForm] DateTime referenceDate, [FromForm] int horizonDays)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        var command = new UploadCustomMapCommand
        {
            Name = name,
            ReferenceDate = referenceDate,
            HorizonDays = horizonDays,
            FileName = file.FileName,
            FileStream = file.OpenReadStream(),
            ContentType = file.ContentType
        };

        var scenarioId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetScenarios), new { id = scenarioId }, scenarioId);
    }

    [HttpPost("custom-maps/blend")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> BlendCustomMap([FromBody] BlendCustomMapCommand command)
    {
        var scenarioId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetScenarios), new { id = scenarioId }, scenarioId);
    }

    [HttpGet("metadata")]
    public async Task<IActionResult> GetForecastMetadata()
    {
        var query = new GetForecastMetadataQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    [HttpGet("ena")]
    [ProducesResponseType(typeof(IEnumerable<EnaResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnaResults([FromQuery] string? submarket, [FromQuery] int offsetDays = 0)
    {
        var query = new GetEnaResultsQuery 
        { 
            Submarket = submarket, 
            OffsetDays = offsetDays 
        };
        
        var results = await _mediator.Send(query);
        return Ok(results);
    }

    [HttpGet("executions")]
    [ProducesResponseType(typeof(IEnumerable<ModelExecutionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExecutions()
    {
        var query = new GetModelExecutionsQuery();
        var results = await _mediator.Send(query);
        return Ok(results);
    }

    [HttpGet("exports/{executionId}")]
    [ProducesResponseType(typeof(IEnumerable<ExportFileDto>), StatusCodes.Status200OK)]
    public IActionResult GetExports(Guid executionId)
    {
        return Ok(Array.Empty<ExportFileDto>());
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
