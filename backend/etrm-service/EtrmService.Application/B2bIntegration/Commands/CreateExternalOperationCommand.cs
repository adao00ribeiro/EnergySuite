using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.B2bIntegration.Commands;

public class CreateExternalOperationCommand : IRequest<Guid>
{
    public Guid CounterpartyId { get; set; }
    public OperationType Type { get; set; }
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ExternalPlatform { get; set; } = string.Empty; // e.g., BBCE, N5X
    public string ExternalId { get; set; } = string.Empty;
}

public class CreateExternalOperationCommandHandler : IRequestHandler<CreateExternalOperationCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateExternalOperationCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateExternalOperationCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        // Fetch a default portfolio for the tenant (simplified for this context)
        var defaultPortfolio = await _context.Portfolios
            .FirstOrDefaultAsync(p => p.TenantId == tenantId, cancellationToken);
            
        if (defaultPortfolio == null)
            throw new InvalidOperationException("No default portfolio found for tenant.");

        // Create a Ticket for this external operation
        var ticket = new Ticket(request.ExternalId, tenantId);
        _context.Tickets.Add(ticket);

        var operation = new Operation(
            ticketId: ticket.Id,
            portfolioId: defaultPortfolio.Id,
            counterpartyId: request.CounterpartyId,
            type: request.Type,
            volumeMwMed: request.VolumeMwMed,
            price: request.Price,
            startDate: request.StartDate,
            endDate: request.EndDate,
            tenantId: tenantId
        );
        
        // External trades are usually published immediately or bypass draft state
        operation.ChangeState(OperationState.Published);

        _context.Operations.Add(operation);

        // Add audit log for tracking external platform
        var auditLog = new AuditLog(
            "Operation", 
            operation.Id.ToString(), 
            "Created", 
            $"{{ \"Source\": \"{request.ExternalPlatform}\", \"ExternalId\": \"{request.ExternalId}\" }}", 
            "System", 
            tenantId
        );
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return operation.Id;
    }
}
