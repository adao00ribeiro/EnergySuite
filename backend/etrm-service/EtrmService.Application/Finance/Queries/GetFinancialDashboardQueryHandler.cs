using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Finance.DTOs;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.Finance.Queries;

public class GetFinancialDashboardQueryHandler : IRequestHandler<GetFinancialDashboardQuery, FinancialDashboardDto>
{
    private readonly IEtrmDbContext _context;

    public GetFinancialDashboardQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<FinancialDashboardDto> Handle(GetFinancialDashboardQuery request, CancellationToken cancellationToken)
    {
        var openSettlements = await _context.FinancialSettlements
            .AsNoTracking()
            .Where(fs => fs.Status == FinancialSettlementStatus.Open)
            .OrderBy(fs => fs.DueDate)
            .Select(fs => new OpenSettlementDto
            {
                Id = fs.Id,
                CounterpartyId = fs.CounterpartyId,
                CounterpartyName = fs.Counterparty.TradeName != null && fs.Counterparty.TradeName != string.Empty
                    ? fs.Counterparty.TradeName
                    : fs.Counterparty.CorporateName,
                ReferenceMonth = fs.ReferenceMonth,
                Type = fs.Type.ToString(),
                Amount = fs.Amount,
                DueDate = fs.DueDate,
                Status = fs.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        var billableStates = new[]
        {
            OperationState.Approved,
            OperationState.Published,
            OperationState.Official
        };

        var operationsToBill = await _context.Operations
            .AsNoTracking()
            .Where(o => billableStates.Contains(o.State) && !_context.Billings.Any(b => b.OperationId == o.Id))
            .OrderBy(o => o.StartDate)
            .Select(o => new OperationToBillDto
            {
                Id = o.Id,
                CounterpartyId = o.CounterpartyId,
                CounterpartyName = o.Counterparty.TradeName != null && o.Counterparty.TradeName != string.Empty
                    ? o.Counterparty.TradeName
                    : o.Counterparty.CorporateName,
                OperationType = o.Type.ToString(),
                VolumeMwMed = o.VolumeMwMed,
                Price = o.Price,
                StartDate = o.StartDate,
                EndDate = o.EndDate
            })
            .ToListAsync(cancellationToken);

        var totalPayable = openSettlements.Where(fs => fs.Type == FinancialSettlementType.Payable.ToString()).Sum(fs => fs.Amount);
        var totalReceivable = openSettlements.Where(fs => fs.Type == FinancialSettlementType.Receivable.ToString()).Sum(fs => fs.Amount);

        return new FinancialDashboardDto
        {
            OpenSettlements = openSettlements,
            OperationsToBill = operationsToBill,
            Totals = new FinanceTotalsDto
            {
                TotalPayable = totalPayable,
                TotalReceivable = totalReceivable,
                NetBalance = totalReceivable - totalPayable
            }
        };
    }
}
