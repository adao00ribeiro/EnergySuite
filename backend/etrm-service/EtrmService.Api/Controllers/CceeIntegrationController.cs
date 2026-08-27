using System;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.Api.Controllers.Shared;
using EtrmService.Application.CceeIntegration.Commands;
using EtrmService.Application.CceeIntegration.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.Api.Controllers;

[ApiVersion("1.0")]
public class CceeIntegrationController : ApiControllerBase
{
    public CceeIntegrationController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("export-cceal")]
    public async Task<IActionResult> ExportCceal([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        var query = new GenerateCcealXmlQuery { PeriodStart = start, PeriodEnd = end };
        var xml = await Mediator.Send(query);
        
        var bytes = Encoding.UTF8.GetBytes(xml);
        return File(bytes, "application/xml", $"cceal_{start:yyyyMMdd}_{end:yyyyMMdd}.xml");
    }

    [HttpPost("upload-cliqccee")]
    public async Task<IActionResult> UploadCliqCcee(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var command = new ProcessCliqCceeCsvCommand { CsvStream = stream };
        
        var importedCount = await Mediator.Send(command);
        return Ok(new { Message = $"Successfully processed {importedCount} records." });
    }

    [HttpGet("comparisons")]
    public async Task<IActionResult> GetComparisons()
    {
        var query = new GetCceeComparisonsQuery();
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("generate-adjustments")]
    public async Task<IActionResult> GenerateAdjustments([FromBody] GenerateAdjustmentXmlCommand command)
    {
        if (command.ComparisonIds == null || command.ComparisonIds.Count == 0)
            return BadRequest("No comparisons provided.");

        var xml = await Mediator.Send(command);
        
        if (string.IsNullOrEmpty(xml))
            return BadRequest("No pending comparisons found for the provided IDs.");
            
        var bytes = Encoding.UTF8.GetBytes(xml);
        return File(bytes, "application/xml", $"cceal_adjustments_{DateTime.UtcNow:yyyyMMddHHmmss}.xml");
    }
}
