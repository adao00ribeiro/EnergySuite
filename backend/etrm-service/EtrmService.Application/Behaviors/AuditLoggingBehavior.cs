using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Behaviors;

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _logger.LogInformation("[AUDIT] Executing Command/Query: {RequestName}", requestName);
        
        var timer = Stopwatch.StartNew();
        
        var response = await next();
        
        timer.Stop();
        
        _logger.LogInformation("[AUDIT] Command/Query {RequestName} executed successfully in {ElapsedMilliseconds} ms", requestName, timer.ElapsedMilliseconds);
        
        return response;
    }
}
