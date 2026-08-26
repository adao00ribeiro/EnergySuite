using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Operations.Commands;

public class ChangeOperationStateCommandHandler : IRequestHandler<ChangeOperationStateCommand, bool>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangeOperationStateCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ChangeOperationStateCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);
            
        if (operation == null)
            return false;

        var oldState = operation.State.ToString();
        operation.ChangeState(request.NewState);
        
        var changes = JsonSerializer.Serialize(new { OldState = oldState, NewState = request.NewState.ToString() });
        var auditLog = new AuditLog("Operation", operation.Id.ToString(), "StateChanged", changes, _currentUserService.UserId ?? "system", _currentUserService.TenantId);
        
        _context.AuditLogs.Add(auditLog);
        
        await _context.SaveChangesAsync(cancellationToken);
        
        return true;
    }
}
