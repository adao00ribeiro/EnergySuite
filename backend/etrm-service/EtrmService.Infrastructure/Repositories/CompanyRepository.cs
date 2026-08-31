using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly EtrmDbContext _dbContext;

    public CompanyRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _dbContext.Companies.AddAsync(company, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Companies
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
