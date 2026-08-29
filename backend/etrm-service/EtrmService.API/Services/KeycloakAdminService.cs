using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Services;

public class KeycloakUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime? LastAccess { get; set; }
    public string Status { get; set; } = "Active";
}

public class KeycloakRoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public interface IKeycloakAdminService
{
    Task<List<KeycloakUserDto>> GetUsersAsync(CancellationToken cancellationToken);
    Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken);
}

public class KeycloakAdminService : IKeycloakAdminService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<KeycloakUserDto>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var realm = Realm;
        var usersResponse = await client.GetAsync($"{AdminBaseUrl}/admin/realms/{realm}/users", cancellationToken);
        usersResponse.EnsureSuccessStatusCode();

        var usersJson = await usersResponse.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(usersJson);
        var users = new List<KeycloakUserDto>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            var userId = element.TryGetProperty("id", out var idProp) ? idProp.GetString() : string.Empty;
            var username = element.TryGetProperty("username", out var nameProp) ? nameProp.GetString() : string.Empty;
            var email = element.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : string.Empty;
            var enabled = !element.TryGetProperty("enabled", out var enabledProp) || enabledProp.GetBoolean();

            DateTime? lastAccess = null;
            if (element.TryGetProperty("lastAccess", out var lastAccessProp) && lastAccessProp.TryGetInt64(out var epochMillis))
            {
                lastAccess = DateTimeOffset.FromUnixTimeMilliseconds(epochMillis).UtcDateTime;
            }

            var roles = new List<string>();
            var roleMappingsResponse = await client.GetAsync($"{AdminBaseUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm", cancellationToken);
            if (roleMappingsResponse.IsSuccessStatusCode)
            {
                var rolesJson = await roleMappingsResponse.Content.ReadAsStringAsync(cancellationToken);
                using var rolesDocument = JsonDocument.Parse(rolesJson);
                foreach (var role in rolesDocument.RootElement.EnumerateArray())
                {
                    if (role.TryGetProperty("name", out var roleNameProp))
                        roles.Add(roleNameProp.GetString() ?? string.Empty);
                }
            }

            users.Add(new KeycloakUserDto
            {
                Id = userId ?? string.Empty,
                Username = username ?? string.Empty,
                Email = email ?? string.Empty,
                Roles = roles,
                LastAccess = lastAccess,
                Status = enabled ? "Active" : "Suspended"
            });
        }

        return users;
    }

    public async Task UpdateUserRolesAsync(string userId, IEnumerable<string> roles, CancellationToken cancellationToken)
    {
        var token = await GetAccessTokenAsync(cancellationToken);
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var realm = Realm;

        var rolesResponse = await client.GetAsync($"{AdminBaseUrl}/admin/realms/{realm}/roles", cancellationToken);
        rolesResponse.EnsureSuccessStatusCode();

        var rolesJson = await rolesResponse.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(rolesJson);

        var roleNames = roles.Where(r => !string.IsNullOrWhiteSpace(r)).ToHashSet();
        var payload = new List<KeycloakRoleDto>();

        foreach (var element in document.RootElement.EnumerateArray())
        {
            if (!element.TryGetProperty("name", out var nameProp))
                continue;

            var name = nameProp.GetString();
            if (name == null || !roleNames.Contains(name))
                continue;

            var id = element.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;
            payload.Add(new KeycloakRoleDto { Id = id, Name = name });
        }

        var content = JsonContent.Create(payload);
        var response = await client.PutAsync($"{AdminBaseUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private string AdminBaseUrl => (_configuration["Keycloak:AdminBaseUrl"] ?? "http://localhost:8080").TrimEnd('/');

    private string Realm => _configuration["Keycloak:Realm"] ?? "EnergySuite";

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var clientId = _configuration["Keycloak:AdminClientId"];
        var clientSecret = _configuration["Keycloak:AdminClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            throw new InvalidOperationException("Keycloak admin client credentials (Keycloak:AdminClientId/AdminClientSecret) are not configured.");

        var client = _httpClientFactory.CreateClient();
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret)
        });

        var response = await client.PostAsync($"{AdminBaseUrl}/realms/{Realm}/protocol/openid-connect/token", form, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var document = JsonDocument.Parse(content);
        if (!document.RootElement.TryGetProperty("access_token", out var tokenProp))
            throw new InvalidOperationException("Keycloak token response did not include an access_token.");

        return tokenProp.GetString() ?? string.Empty;
    }
}