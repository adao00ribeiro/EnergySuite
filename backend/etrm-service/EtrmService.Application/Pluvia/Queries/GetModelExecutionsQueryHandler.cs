using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Pluvia.Queries;

public class GetModelExecutionsQueryHandler : IRequestHandler<GetModelExecutionsQuery, IEnumerable<ModelExecutionDto>>
{
    private readonly IEtrmDbContext _context;

    public GetModelExecutionsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ModelExecutionDto>> Handle(GetModelExecutionsQuery request, CancellationToken cancellationToken)
    {
        var executions = await _context.ModelExecutions
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAt)
            .Take(10)
            .Select(x => new ModelExecutionDto
            {
                Id = x.Id,
                ModelName = x.ModelType.ToString(),
                Status = x.Status.ToString().ToLower(),
                Accuracy = x.Status == ExecutionStatus.Completed ? "Concluída" 
                    : x.Status == ExecutionStatus.Running ? "Em execução" 
                    : x.Status == ExecutionStatus.Failed ? "Falhou" 
                    : "Pendente",
                StartedAt = x.StartedAt,
                CompletedAt = x.CompletedAt
            })
            .ToListAsync(cancellationToken);

        return executions;
    }
}
