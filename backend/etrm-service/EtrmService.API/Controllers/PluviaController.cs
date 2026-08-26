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
        // Mocking exports for the UI
        var minioBaseUrl = Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "http://localhost:9000";
        var bucket = "datalake";
        
        var mockExports = new List<ExportFileDto>
        {
            new ExportFileDto { FileName = "PREVS.rv0", FileType = "PREVS", SizeBytes = 1024 * 45, DownloadUrl = $"{minioBaseUrl}/{bucket}/exports/{executionId}/PREVS.rv0" },
            new ExportFileDto { FileName = "ENA.rv0", FileType = "ENA", SizeBytes = 1024 * 12, DownloadUrl = $"{minioBaseUrl}/{bucket}/exports/{executionId}/ENA.rv0" },
            new ExportFileDto { FileName = "VNA.rv0", FileType = "VNA", SizeBytes = 1024 * 8, DownloadUrl = $"{minioBaseUrl}/{bucket}/exports/{executionId}/VNA.rv0" },
            new ExportFileDto { FileName = "DADVAZ.rv0", FileType = "DADVAZ", SizeBytes = 1024 * 55, DownloadUrl = $"{minioBaseUrl}/{bucket}/exports/{executionId}/DADVAZ.rv0" }
        };

        return Ok(mockExports);
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
