using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;

namespace EtrmService.Domain.Interfaces;

public interface IContractRepository
{
    Task AddAsync(Contract contract, CancellationToken cancellationToken = default);
}
