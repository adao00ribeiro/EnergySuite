using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.CommercialRegistry.DTOs;

namespace EtrmService.Application.CommercialRegistry.Queries;

public class GetCompaniesQueryHandler : IRequestHandler<GetCompaniesQuery, List<CompanyDto>>
{
    private readonly IEtrmDbContext _context;

    public GetCompaniesQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<List<CompanyDto>> Handle(GetCompaniesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Companies
            .AsNoTracking()
            .Select(c => new CompanyDto
            {
                Id = c.Id,
                Cnpj = c.Cnpj,
                CorporateName = c.CorporateName,
                TradeName = c.TradeName,
                Category = c.Category.ToString(),
                City = c.Address != null ? c.Address.City : null,
                State = c.Address != null ? c.Address.State : null,
                CceeCode = c.CceeCode,
                CceeProfile = c.CceeProfile
            })
            .ToListAsync(cancellationToken);
    }
}
