using System;
using System.Threading.Tasks;
using EtrmService.Application.Prospect.Commands;
using EtrmService.Application.Prospect.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProspectController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProspectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("studies")]
    public async Task<IActionResult> GetStudies()
    {
        // Mock TenantId for now. Real implementation uses User context.
        var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var query = new GetStudiesQuery { TenantId = tenantId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("studies")]
    public async Task<IActionResult> CreateStudy([FromBody] CreateStudyCommand command)
    {
        // Mock User and Tenant for now.
        command.TenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        command.UserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        var studyId = await _mediator.Send(command);
        return Created($"/api/v1/prospect/studies/{studyId}", new { Id = studyId });
    }
}
