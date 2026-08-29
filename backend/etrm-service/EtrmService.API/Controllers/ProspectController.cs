using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Prospect.Commands;
using EtrmService.Application.Prospect.DTOs;
using EtrmService.Application.Prospect.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers;

[ApiController]
[Route("api/v1/prospect")]
[Authorize]
public class ProspectController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ProspectController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpPost("studies")]
    [ProducesResponseType(typeof(StudyDto), 200)]
    public async Task<IActionResult> CreateStudy([FromBody] CreateStudyCommand command)
    {
        command.TenantId = _currentUser.TenantId;
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("studies")]
    [ProducesResponseType(typeof(List<StudyDto>), 200)]
    public async Task<IActionResult> GetStudies()
    {
        var query = new GetStudiesQuery { TenantId = _currentUser.TenantId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("studies/{id}/execute")]
    [ProducesResponseType(typeof(object), 202)]
    public async Task<IActionResult> ExecuteStudy(Guid id)
    {
        var command = new ExecuteStudyCommand
        {
            StudyId = id,
            TenantId = _currentUser.TenantId
        };

        await _mediator.Send(command);
        return Accepted(new { Message = "Execution queued successfully." });
    }

    [HttpGet("studies/{id}/results")]
    [ProducesResponseType(typeof(StudyResultResponseDto), 200)]
    public async Task<IActionResult> GetStudyResults(Guid id)
    {
        var query = new GetStudyResultsQuery
        {
            StudyId = id,
            TenantId = _currentUser.TenantId
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("studies/{id}/clone")]
    [ProducesResponseType(typeof(StudyDto), 200)]
    public async Task<IActionResult> CloneStudy(Guid id)
    {
        var command = new CloneStudyCommand
        {
            StudyId = id,
            TenantId = _currentUser.TenantId
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
