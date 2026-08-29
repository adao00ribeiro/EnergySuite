using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IEtrmDbContext _context;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;

        public SettingsController(ILogger<SettingsController> logger, IEtrmDbContext context, ICurrentUserService currentUser, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _currentUser = currentUser;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var settings = await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.TenantId == _currentUser.TenantId)
                .ToListAsync(cancellationToken);

            return Ok(new
            {
                theme = GetValue(settings, "theme"),
                language = GetValue(settings, "language"),
                timezone = GetValue(settings, "timezone")
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto settings, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating settings for tenant {TenantId}: {Settings}", _currentUser.TenantId, settings);

            await UpsertAsync("theme", settings.Theme, cancellationToken);
            await UpsertAsync("language", settings.Language, cancellationToken);
            await UpsertAsync("timezone", settings.Timezone, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return Ok();
        }

        [HttpPost("m2m-tokens")]
        public IActionResult GenerateM2MToken()
        {
            _logger.LogInformation("Generating M2M API Key");
            var token = BuildM2MToken();
            return Ok(new
            {
                id = Guid.NewGuid().ToString(),
                name = "Nova Chave M2M",
                token,
                createdAt = DateTime.UtcNow
            });
        }

        private string BuildM2MToken()
        {
            var secret = _configuration["Jwt:M2MSecret"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("JWT M2M secret (Jwt:M2MSecret) não configurado.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiryDays = _configuration["Jwt:M2MTtlDays"] != null && int.TryParse(_configuration["Jwt:M2MTtlDays"], out var days)
                ? days
                : 30;

            var claims = new[]
            {
                new Claim("tenant_id", _currentUser.TenantId.ToString()),
                new Claim(ClaimTypes.Role, "B2B"),
                new Claim(JwtRegisteredClaimNames.Sub, _currentUser.UserId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "EtrmService",
                audience: _configuration["Jwt:Audience"] ?? "EtrmService-B2B",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(expiryDays),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task UpsertAsync(string key, string? value, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var existing = await _context.AppSettings
                .FirstOrDefaultAsync(s => s.TenantId == _currentUser.TenantId && s.Key == key, cancellationToken);

            if (existing == null)
                _context.AppSettings.Add(new AppSetting(_currentUser.TenantId, key, value));
            else
                existing.UpdateValue(value);
        }

        private static string GetValue(IReadOnlyCollection<AppSetting> settings, string key)
            => settings.FirstOrDefault(s => s.Key == key)?.Value ?? string.Empty;
    }

    public class SettingsDto
    {
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public string? Timezone { get; set; }
    }
}