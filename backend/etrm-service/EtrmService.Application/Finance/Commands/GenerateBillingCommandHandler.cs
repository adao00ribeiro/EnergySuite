using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Finance.Commands;

public class GenerateBillingCommandHandler : IRequestHandler<GenerateBillingCommand, Guid>
{
    private readonly IEtrmDbContext _context;

    public GenerateBillingCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(GenerateBillingCommand request, CancellationToken cancellationToken)
    {
        var operation = await _context.Operations
            .FirstOrDefaultAsync(o => o.Id == request.OperationId, cancellationToken);

        if (operation == null)
            throw new InvalidOperationException($"Operation {request.OperationId} not found.");

        var billing = new Billing(
            request.OperationId,
            request.ReferenceMonth,
            request.CalculatedVolume,
            request.AppliedPrice,
            request.TaxesAmount
        );

        _context.Billings.Add(billing);
        
        // Define due date arbitrarily as the 15th of the following month for simplicity
        var monthParts = request.ReferenceMonth.Split('-');
        var dueDate = new DateTime(int.Parse(monthParts[0]), int.Parse(monthParts[1]), 15).AddMonths(1);

        // Determine type based on operation type (Purchase = Payable, Sale = Receivable)
        var settlementType = operation.Type == OperationType.Purchase 
            ? FinancialSettlementType.Payable 
            : FinancialSettlementType.Receivable;

        var settlement = new FinancialSettlement(
            billing.Id,
            operation.CounterpartyId,
            operation.TenantId,
            settlementType,
            billing.NetAmount,
            dueDate,
            request.ReferenceMonth
        );

        _context.FinancialSettlements.Add(settlement);

        await _context.SaveChangesAsync(cancellationToken);

        return billing.Id;
    }
}
