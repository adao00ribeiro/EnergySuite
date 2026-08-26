using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;

namespace EtrmService.Application.Operations.Commands;

public class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateOperationCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = new Operation(
            request.TicketId,
            request.PortfolioId,
            request.CounterpartyId,
            request.Type,
            request.VolumeMwMed,
            request.Price,
            request.StartDate,
            request.EndDate,
            _currentUserService.TenantId
        );

        _context.Operations.Add(operation);
        
        var changes = JsonSerializer.Serialize(new { State = "Draft", Volume = request.VolumeMwMed, Price = request.Price });
        var auditLog = new AuditLog("Operation", operation.Id.ToString(), "Created", changes, _currentUserService.UserId ?? "system", _currentUserService.TenantId);
        
        _context.AuditLogs.Add(auditLog);
        
        await _context.SaveChangesAsync(cancellationToken);

        return operation.Id;
    }
}
