using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            // Retorna configurações mockadas por enquanto (até termos a entidade pronta no Application/Domain)
            return Ok(new
            {
                theme = "dark",
                language = "pt-BR",
                timezone = "America/Sao_Paulo"
            });
        }

        [HttpPut]
        public IActionResult UpdateSettings([FromBody] object settingsDto)
        {
            _logger.LogInformation("Updating user settings: {settings}", settingsDto);
            // Salvar no banco
            return Ok();
        }

        [HttpPost("m2m-tokens")]
        public IActionResult GenerateM2MToken()
        {
            _logger.LogInformation("Generating M2M API Key");
            var token = new
            {
                id = Guid.NewGuid().ToString(),
                name = "Nova Chave M2M",
                token = "ey..." + Guid.NewGuid().ToString("N"),
                createdAt = DateTime.UtcNow
            };
            return Ok(token);
        }
    }
}
