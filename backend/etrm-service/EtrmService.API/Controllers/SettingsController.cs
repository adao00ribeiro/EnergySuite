using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Settings.Commands;
using EtrmService.Application.Settings.DTOs;
using EtrmService.Application.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;


namespace EtrmService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUser;
        private readonly IConfiguration _configuration;

        public SettingsController(
            ILogger<SettingsController> logger,
            IMediator mediator,
            ICurrentUserService currentUser,
            IConfiguration configuration)
        {
            _logger = logger;
            _mediator = mediator;
            _currentUser = currentUser;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        {
            var settings = await _mediator.Send(new GetSettingsQuery(_currentUser.TenantId), cancellationToken);
            return Ok(settings);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettings([FromBody] SettingsDto settings, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating settings for tenant {TenantId}: {Settings}", _currentUser.TenantId, settings);
            var result = await _mediator.Send(new UpdateSettingsCommand(_currentUser.TenantId, settings), cancellationToken);
            return Ok(result);
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
    }
}