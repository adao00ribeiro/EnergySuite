using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace EtrmService.Application.Strategies.Queries;

public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "Draft", "Approved", "Inactive"
}

public class GetStrategiesQuery : IRequest<List<StrategyDto>>
{
    public Guid TenantId { get; set; }
}

public class GetStrategiesQueryHandler : IRequestHandler<GetStrategiesQuery, List<StrategyDto>>
{
    public async Task<List<StrategyDto>> Handle(GetStrategiesQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken); // Simula busca no banco
        
        // Mocking Data for Kanban
        return new List<StrategyDto>
        {
            new StrategyDto { Id = Guid.NewGuid(), Name = "Arbitragem Sul x SE", Description = "Compra no Sul e venda no SE aproveitando spread", Status = "Approved" },
            new StrategyDto { Id = Guid.NewGuid(), Name = "Hedge de Inverno", Description = "Proteção contra preços em época de seca", Status = "Draft" },
            new StrategyDto { Id = Guid.NewGuid(), Name = "Venda Excedente Eólica", Description = "Desovar excedentes do NE", Status = "Approved" },
            new StrategyDto { Id = Guid.NewGuid(), Name = "Especulação Curto Prazo", Description = "Day trade no PLD", Status = "Inactive" }
        };
    }
}
