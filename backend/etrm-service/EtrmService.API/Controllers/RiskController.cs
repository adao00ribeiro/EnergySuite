using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using EtrmService.Application.Risk.Commands;

namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/risk")]
    public class RiskController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RiskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("mark-to-market")]
        public async Task<IActionResult> CalculateMarkToMarket([FromBody] CalculateMarkToMarketCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
