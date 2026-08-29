using System;
using System.Linq;
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
    /// <summary>Tenant do sistema (integração em background, sem usuário autenticado).</summary>
    public Guid? TenantId { get; set; }
    public string CounterpartyCode { get; set; } = string.Empty; // CceeCode/CceeAcronym da empresa cadastrada
    public OperationType Type { get; set; }
    public decimal VolumeMwMed { get; set; }
    public decimal Price { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string ExternalPlatform { get; set; } = string.Empty; // e.g., CCEE, BBCE
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
        var tenantId = request.TenantId ?? _currentUserService.TenantId;

        // Regra Sprint 9: criar draft somente se a contraparte existir no cadastro;
        // caso contrário, rejeitar (exceção honesta logada pelo chamador).
        var companiesQuery = request.TenantId.HasValue
            ? _context.Companies.IgnoreQueryFilters().Where(c => c.TenantId == tenantId)
            : _context.Companies;

        var counterparty = await companiesQuery.FirstOrDefaultAsync(c =>
            (c.CceeCode != null && c.CceeCode.Equals(request.CounterpartyCode, StringComparison.OrdinalIgnoreCase)) ||
            (c.CceeAcronym != null && c.CceeAcronym.Equals(request.CounterpartyCode, StringComparison.OrdinalIgnoreCase)),
            cancellationToken);

        if (counterparty == null)
            throw new InvalidOperationException($"Counterparty not found for external code '{request.CounterpartyCode}'.");

        var portfoliosQuery = request.TenantId.HasValue
            ? _context.Portfolios.IgnoreQueryFilters().Where(p => p.TenantId == tenantId)
            : _context.Portfolios;

        var defaultPortfolio = await portfoliosQuery
            .FirstOrDefaultAsync(cancellationToken);

        if (defaultPortfolio == null)
            throw new InvalidOperationException("No default portfolio found for tenant.");

        // Cria um Ticket para a operação externa, referenciando o id real da fonte (ExternalId).
        var ticket = new Ticket(request.ExternalId, tenantId);
        _context.Tickets.Add(ticket);

        var operation = new Operation(
            ticketId: ticket.Id,
            portfolioId: defaultPortfolio.Id,
            counterpartyId: counterparty.Id,
            type: request.Type,
            volumeMwMed: request.VolumeMwMed,
            price: request.Price,
            startDate: request.StartDate,
            endDate: request.EndDate,
            tenantId: tenantId
        );

        // Regra Sprint 9: trades externos entram como rascunho para aprovação do backoffice.
        _context.Operations.Add(operation);

        // Audit log de origem (plataforma externa + id real da fonte).
        var auditLog = new AuditLog(
            "Operation",
            operation.Id.ToString(),
            "Created",
            $"{{\"Source\":\"{request.ExternalPlatform}\",\"ExternalId\":\"{request.ExternalId}\",\"State\":\"Draft\"}}",
            "System",
            tenantId
        );
        _context.AuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);

        return operation.Id;
    }
}