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
    private readonly ICurrentUserService _currentUserService;

    public CreateContractCommandHandler(IContractRepository repository, IEventPublisher eventPublisher, ICurrentUserService currentUserService)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
        _currentUserService = currentUserService;
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
            request.OptionPremium,
            _currentUserService.TenantId
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
            contract.OptionPremium,
            contract.TenantId
        );
        
        await _eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        return contract.Id;
    }
}
