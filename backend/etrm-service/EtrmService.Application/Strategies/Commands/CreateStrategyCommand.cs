using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;

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
    private readonly IEtrmDbContext _context;

    public CreateStrategyCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStrategyCommand request, CancellationToken cancellationToken)
    {
        var strategy = new Strategy(request.Name, request.Description, request.TenantId);

        _context.Strategies.Add(strategy);
        await _context.SaveChangesAsync(cancellationToken);

        return strategy.Id;
    }
}
