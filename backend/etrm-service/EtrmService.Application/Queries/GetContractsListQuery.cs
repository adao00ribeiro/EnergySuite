using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Queries.DTOs;
using EtrmService.Domain.Interfaces;

namespace EtrmService.Application.Queries;

public record GetContractsListQuery : IRequest<IEnumerable<ContractDto>>;

public class GetContractsListQueryHandler : IRequestHandler<GetContractsListQuery, IEnumerable<ContractDto>>
{
    private readonly IContractRepository _repository;

    public GetContractsListQueryHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ContractDto>> Handle(GetContractsListQuery request, CancellationToken cancellationToken)
    {
        var contracts = await _repository.GetAllAsync(cancellationToken);

        return contracts.Select(c => new ContractDto
        {
            Id = c.Id,
            CounterpartyName = c.CounterpartyName,
            Type = c.Type.ToString(),
            Submarket = c.Submarket.ToString(),
            VolumeMwMed = c.VolumeMwMed,
            Price = c.Price,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
    }
}
