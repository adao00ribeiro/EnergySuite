using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using EtrmService.Application.Interfaces;
using EtrmService.Application.IntegrationEvents;

namespace EtrmService.Application.Commands;

public class CreateContractCommandHandler : IRequestHandler<CreateContractCommand, Guid>
{
    private readonly IContractRepository _repository;
    private readonly IEventPublisher _eventPublisher;

    public CreateContractCommandHandler(IContractRepository repository, IEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
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
            request.EndDate,
            request.StrikePrice,
            request.OptionPremium
        );

        await _repository.AddAsync(contract, cancellationToken);

        // Publicar evento de integração no Kafka
        var integrationEvent = new ContractCreatedIntegrationEvent(
            contract.Id,
            contract.CounterpartyName,
            contract.Type,
            contract.Submarket,
            contract.VolumeMwMed,
            contract.Price,
            contract.StartDate,
            contract.EndDate,
            DateTime.UtcNow,
            contract.StrikePrice,
            contract.OptionPremium
        );
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return contract.Id;
    }
}
