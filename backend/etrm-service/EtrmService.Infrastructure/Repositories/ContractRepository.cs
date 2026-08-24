using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Infrastructure.Data;

namespace EtrmService.Infrastructure.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly EtrmDbContext _dbContext;

    public ContractRepository(EtrmDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Contract contract, CancellationToken cancellationToken = default)
    {
        await _dbContext.Contracts.AddAsync(contract, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
