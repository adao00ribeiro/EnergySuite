using System;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.CceeIntegration.Commands;
using EtrmService.Application.CceeIntegration.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
[Authorize]
public class CceeIntegrationController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public CceeIntegrationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("comparisons")]
    public async Task<IActionResult> GetComparisons()
    {
        var result = await _mediator.Send(new GetCceeComparisonsQuery());
        return Ok(result);
    }

    [HttpPost("upload-cliqccee")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadCliqCcee(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        var command = new ProcessCliqCceeCsvCommand
        {
            CsvStream = file.OpenReadStream()
        };

        var importedCount = await _mediator.Send(command);
        return Ok(new { importedCount });
    }

    [HttpPost("export-cceal")]
    public async Task<IActionResult> ExportCceal([FromQuery] DateTime periodStart, [FromQuery] DateTime periodEnd)
    {
        var query = new GenerateCcealXmlQuery
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd
        };

        var xml = await _mediator.Send(query);
        return Content(xml, "application/xml");
    }

    [HttpPost("generate-adjustments")]
    public async Task<IActionResult> GenerateAdjustments([FromBody] GenerateAdjustmentXmlCommand command)
    {
        var xml = await _mediator.Send(command);
        return Content(xml, "application/xml");
    }
}
