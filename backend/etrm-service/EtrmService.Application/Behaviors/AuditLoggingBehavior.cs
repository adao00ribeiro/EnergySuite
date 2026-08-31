using System.Diagnostics;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Behaviors;

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;

    public AuditLoggingBehavior(
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[AUDIT] Executing Command/Query: {RequestName}", requestName);

        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next();

            timer.Stop();
            _logger.LogInformation("[AUDIT] Command/Query {RequestName} executed successfully in {ElapsedMilliseconds} ms", requestName, timer.ElapsedMilliseconds);

            await PersistAuditAsync(request, "Executed", timer.ElapsedMilliseconds, cancellationToken);

            return response;
        }
        catch (System.Exception ex)
        {
            timer.Stop();
            _logger.LogError(ex, "[AUDIT] Command/Query {RequestName} failed after {ElapsedMilliseconds} ms", requestName, timer.ElapsedMilliseconds);

            await PersistAuditAsync(request, $"Failed: {ex.GetType().Name}", timer.ElapsedMilliseconds, cancellationToken);

            throw;
        }
    }

    private async Task PersistAuditAsync(TRequest request, string action, long elapsedMs, CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = new AuditLog(
                entityName: typeof(TRequest).Name,
                entityId: ExtractEntityId(request) ?? string.Empty,
                action: action,
                changesJson: SerializeChanges(request),
                changedBy: _currentUserService.UserId ?? "system",
                tenantId: _currentUserService.TenantId);

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, "[AUDIT] Failed to persist audit log for {RequestName}. Audit failure must not break the operation.", typeof(TRequest).Name);
        }
    }

    private static string? ExtractEntityId(TRequest request)
    {
        var idProp = typeof(TRequest).GetProperty("Id")
            ?? typeof(TRequest).GetProperty("RequestId"); 

        if (idProp == null)
            return null;

        var value = idProp.GetValue(request);
        return value?.ToString();
    }

    private static string SerializeChanges(TRequest request)
    {
        try
        {
            return JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
        catch
        {
            return request?.ToString() ?? string.Empty;
        }
    }
}

