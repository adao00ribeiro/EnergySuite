using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Finance.Commands;

public class ExecuteAccountOffsetCommandHandler : IRequestHandler<ExecuteAccountOffsetCommand, Guid?>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ExecuteAccountOffsetCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid?> Handle(ExecuteAccountOffsetCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;

        // Fetch all open settlements for the given counterparty and reference month
        var openSettlements = await _context.FinancialSettlements
            .Where(fs => fs.CounterpartyId == request.CounterpartyId && 
                         fs.ReferenceMonth == request.ReferenceMonth && 
                         fs.Status == FinancialSettlementStatus.Open)
            .ToListAsync(cancellationToken);

        if (!openSettlements.Any())
            return null; // Nothing to offset

        // Sum payables and receivables
        decimal totalPayable = openSettlements.Where(fs => fs.Type == FinancialSettlementType.Payable).Sum(fs => fs.Amount);
        decimal totalReceivable = openSettlements.Where(fs => fs.Type == FinancialSettlementType.Receivable).Sum(fs => fs.Amount);

        // If totalPayable == 0 or totalReceivable == 0, we can't do an offset
        if (totalPayable == 0 || totalReceivable == 0)
            return null;

        var offsetGroupId = Guid.NewGuid();

        // Mark all original settlements as offset
        foreach (var settlement in openSettlements)
        {
            settlement.MarkAsOffset(offsetGroupId);
        }

        // Calculate residual (net amount)
        decimal residualAmount = 0;
        FinancialSettlementType residualType;

        if (totalPayable > totalReceivable)
        {
            residualAmount = totalPayable - totalReceivable;
            residualType = FinancialSettlementType.Payable;
        }
        else if (totalReceivable > totalPayable)
        {
            residualAmount = totalReceivable - totalPayable;
            residualType = FinancialSettlementType.Receivable;
        }
        else
        {
            // Exact match, no residual needed! They cancel out entirely.
            await _context.SaveChangesAsync(cancellationToken);
            return offsetGroupId;
        }

        // Create the residual settlement
        var firstSettlement = openSettlements.First();
        var residualSettlement = new FinancialSettlement(
            null, // No single billing attached, this is a consolidation
            request.CounterpartyId,
            tenantId,
            residualType,
            residualAmount,
            firstSettlement.DueDate,
            request.ReferenceMonth
        );

        _context.FinancialSettlements.Add(residualSettlement);

        await _context.SaveChangesAsync(cancellationToken);

        return offsetGroupId;
    }
}
