using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Domain.Entities;
// Assuming some mock repository or just returning success for Sprint 3 MVP

namespace EtrmService.Application.Strategies.Commands;

public class CreateStrategyCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TenantId { get; set; }

    public CreateStrategyCommand(string name, string description, Guid tenantId)
    {
        Name = name;
        Description = description;
        TenantId = tenantId;
    }
}

public class CreateStrategyCommandHandler : IRequestHandler<CreateStrategyCommand, Guid>
{
    public async Task<Guid> Handle(CreateStrategyCommand request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken); // Simula gravação no DB
        
        var strategy = new Strategy(request.Name, request.Description, request.TenantId);
        
        // Em um cenário real, injetaríamos IStrategyRepository e salvaríamos.
        // await _repository.AddAsync(strategy, cancellationToken);

        return strategy.Id;
    }
}
