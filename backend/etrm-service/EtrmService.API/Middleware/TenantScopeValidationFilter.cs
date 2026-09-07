using System;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace EtrmService.API.Middleware;

public class TenantScopeValidationFilter : IAsyncActionFilter
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<TenantScopeValidationFilter> _logger;

    public TenantScopeValidationFilter(ICurrentUserService currentUserService, ILogger<TenantScopeValidationFilter> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var tenantId = _currentUserService.TenantId;

            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("Multi-tenancy violation: Authenticated user '{UserId}' attempt without valid TenantId on '{Path}'.",
                    _currentUserService.UserId, context.HttpContext.Request.Path);

                context.Result = new ObjectResult(new
                {
                    error = "Multi-tenancy TenantId header or claim is missing or invalid.",
                    path = context.HttpContext.Request.Path.Value,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            context.HttpContext.Response.Headers["X-Tenant-ID"] = tenantId.ToString();
        }

        await next();
    }
}
