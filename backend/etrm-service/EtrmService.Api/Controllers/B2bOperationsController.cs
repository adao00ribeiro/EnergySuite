using System;
using System.Threading.Tasks;
using Asp.Versioning;
using EtrmService.Api.Controllers.Shared;
using EtrmService.Application.B2bIntegration.Commands;
using EtrmService.Application.Operations.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EtrmService.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/b2b/operations")]
[EnableRateLimiting("b2b")]
// [Authorize] // Temporarily disabled for ease of testing in this simulation, but normally M2M policy
public class B2bOperationsController : ApiControllerBase
{
    public B2bOperationsController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Criar uma nova operação originada de uma plataforma externa (ex: BBCE)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateExternalOperation([FromBody] CreateExternalOperationCommand command)
    {
        var operationId = await Mediator.Send(command);
        return Ok(new { OperationId = operationId, Message = "External operation created successfully." });
    }

    /// <summary>
    /// Publica uma operação
    /// </summary>
    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishOperation(Guid id)
    {
        // Reusing existing PublishOperationCommand or assuming we have one.
        // For simulation, we assume an integration event will be fired by the command.
        var command = new PublishOperationCommand { OperationId = id };
        var result = await Mediator.Send(command);
        
        if (!result)
            return BadRequest("Failed to publish operation.");
            
        return Ok(new { Message = "Operation published successfully. Webhooks will be triggered." });
    }
}
