using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IContractRepository
{
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
    Task<Contract?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Contract>> GetAllAsync(CancellationToken cancellationToken = default);
}
