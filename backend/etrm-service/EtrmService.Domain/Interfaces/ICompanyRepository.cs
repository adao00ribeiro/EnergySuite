using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface ICompanyRepository
{
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Company>> GetAllAsync(CancellationToken cancellationToken = default);
}
