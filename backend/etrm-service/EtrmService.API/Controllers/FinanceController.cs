using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Asp.Versioning;
using EtrmService.API.Controllers.Shared;
using EtrmService.Application.Finance.Commands;
using EtrmService.Application.Finance.DTOs;
using EtrmService.Application.Finance.Queries;

namespace EtrmService.API.Controllers;

[ApiVersion("1.0")]
public class FinanceController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(FinancialDashboardDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Dashboard()
    {
        var result = await _mediator.Send(new GetFinancialDashboardQuery());
        return Ok(result);
    }

    [HttpPost("offset")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Offset([FromBody] ExecuteAccountOffsetCommand command)
    {
        var offsetGroupId = await _mediator.Send(command);
        return Ok(new { offsetGroupId });
    }

    [HttpPost("billings")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GenerateBilling([FromBody] GenerateBillingCommand command)
    {
        var billingId = await _mediator.Send(command);
        return Ok(new { billingId });
    }
}
