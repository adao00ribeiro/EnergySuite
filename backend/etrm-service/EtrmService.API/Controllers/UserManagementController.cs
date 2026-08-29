using System;
using System.Threading.Tasks;
using EtrmService.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize(Roles = "Portfolio Manager")]
    public class UserManagementController : ControllerBase
    {
        private readonly ILogger<UserManagementController> _logger;
        private readonly IKeycloakAdminService _keycloakAdminService;

        public UserManagementController(ILogger<UserManagementController> logger, IKeycloakAdminService keycloakAdminService)
        {
            _logger = logger;
            _keycloakAdminService = keycloakAdminService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Buscando usuários via Keycloak Admin API");
            try
            {
                var users = await _keycloakAdminService.GetUsersAsync(cancellationToken);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao buscar usuários no Keycloak Admin API");
                return StatusCode(StatusCodes.Status502BadGateway, ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status502BadGateway, "Keycloak indisponível", detail: ex.Message));
            }
        }

        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] string[] roles, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Atualizando roles do usuário {id} no Keycloak", id);
            try
            {
                await _keycloakAdminService.UpdateUserRolesAsync(id, roles, cancellationToken);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao atualizar roles do usuário {id} no Keycloak Admin API", id);
                return StatusCode(StatusCodes.Status502BadGateway, ProblemDetailsFactory.CreateProblemDetails(HttpContext, StatusCodes.Status502BadGateway, "Keycloak indisponível", detail: ex.Message));
            }
        }
    }
}