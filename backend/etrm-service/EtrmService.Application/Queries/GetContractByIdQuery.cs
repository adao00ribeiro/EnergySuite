using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Queries.DTOs;
using EtrmService.Domain.Interfaces;

namespace EtrmService.Application.Queries;

public record GetContractByIdQuery(Guid Id) : IRequest<ContractDto?>;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
{
    private readonly IContractRepository _repository;

    public GetContractByIdQueryHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (contract == null)
            return null;

        return new ContractDto
        {
            Id = contract.Id,
            CounterpartyName = contract.CounterpartyName,
            Type = contract.Type.ToString(),
            Submarket = contract.Submarket.ToString(),
            VolumeMwMed = contract.VolumeMwMed,
            Price = contract.Price,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }
}
