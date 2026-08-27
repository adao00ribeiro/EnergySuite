using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserManagementController(ILogger<UserManagementController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            _logger.LogInformation("Buscando usuários via Keycloak Admin API");
            // Exemplo de integração crúa (Raw HttpClient)
            // var client = _httpClientFactory.CreateClient("KeycloakAdmin");
            // var response = await client.GetAsync("/admin/realms/EnergySuite/users");
            
            // Mock data para UI:
            var mockUsers = new[]
            {
                new { id = "u1", username = "jsilva", email = "joao.silva@energy.com", roles = new[]{"Portfolio Manager"}, lastAccess = DateTime.UtcNow, status = "Active" }
            };

            return Ok(mockUsers);
        }

        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] string[] roles)
        {
            _logger.LogInformation("Atualizando roles do usuário {id} no Keycloak", id);
            // Aqui entra a chamada para /admin/realms/EnergySuite/users/{id}/role-mappings/realm
            return Ok();
        }
    }
}
