using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Portfolios.DTOs;

namespace EtrmService.Application.Portfolios.Queries;

public class GetPortfoliosQueryHandler : IRequestHandler<GetPortfoliosQuery, List<PortfolioDto>>
{
    private readonly IEtrmDbContext _context;

    public GetPortfoliosQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<PortfolioDto>> Handle(GetPortfoliosQuery request, CancellationToken cancellationToken)
    {
        return await _context.Portfolios
            .AsNoTracking()
            .Select(p => new PortfolioDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Type = p.Type,
                Responsible = p.Responsible,
                Status = p.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}
