using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Portfolios.Queries;

public class GetDefaultPortfolioIdQueryHandler : IRequestHandler<GetDefaultPortfolioIdQuery, Guid>
{
    private readonly IEtrmDbContext _context;

    public GetDefaultPortfolioIdQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(GetDefaultPortfolioIdQuery request, CancellationToken cancellationToken)
    {
        if (request.ExplicitPortfolioId.HasValue && request.ExplicitPortfolioId.Value != Guid.Empty)
            return request.ExplicitPortfolioId.Value;

        return await _context.Portfolios
            .AsNoTracking()
            .Where(p => p.TenantId == request.TenantId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
