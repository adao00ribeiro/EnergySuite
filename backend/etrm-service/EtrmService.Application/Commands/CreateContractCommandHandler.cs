using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;

namespace EtrmService.Application.Commands;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Guid>
{
    private readonly IContractRepository _repository;

    public CreateContractCommandHandler(IContractRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateContractCommand request, CancellationToken cancellationToken)
    {
        var contract = new Contract(
            request.CounterpartyName,
            request.Type,
            request.Submarket,
            request.VolumeMwMed,
            request.Price,
            request.StartDate,
            request.EndDate
        );

        await _repository.AddAsync(contract, cancellationToken);

        // TODO: Publicar evento ContractCreatedIntegrationEvent no Kafka aqui

        return contract.Id;
    }
}
