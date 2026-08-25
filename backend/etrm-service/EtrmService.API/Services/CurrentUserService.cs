using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using EtrmService.Application.Interfaces;

namespace EtrmService.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
                return Guid.Empty; // Ou lançar exceção dependendo do design (fallback temporário)

            var tenantClaim = user.FindFirst("tenant_id")?.Value 
                              ?? user.FindFirst("azp")?.Value 
                              ?? "00000000-0000-0000-0000-000000000001"; // Fallback temporário para dev
            
            if (Guid.TryParse(tenantClaim, out var tenantId))
                return tenantId;
            
            // Usando azp (Authorized Party / Client ID) do Keycloak como Tenant provisório se for GUID
            // Num SaaS real, configuraríamos um atributo "tenant_id" no usuário do Keycloak
            
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }
    }

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
}
