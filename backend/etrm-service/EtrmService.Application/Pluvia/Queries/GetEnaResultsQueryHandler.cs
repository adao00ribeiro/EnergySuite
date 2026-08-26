using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Pluvia.Queries;

public class GetEnaResultsQueryHandler : IRequestHandler<GetEnaResultsQuery, IEnumerable<EnaResultDto>>
{
    private readonly IEtrmDbContext _context;

    public GetEnaResultsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EnaResultDto>> Handle(GetEnaResultsQuery request, CancellationToken cancellationToken)
    {
        // Pega a data base simulada baseada no offset (Hoje, Ontem, etc.)
        var baseDate = DateTime.UtcNow.Date.AddDays(request.OffsetDays);
        
        var query = _context.HydrologicalResults.AsNoTracking();

        if (!string.IsNullOrEmpty(request.Submarket))
        {
            query = query.Where(x => x.Submarket == request.Submarket);
        }

        // Simula filtro por data de criacao da projecao
        query = query.Where(x => x.CreatedAt.Date <= baseDate);

        var results = await query
            .OrderBy(x => x.TargetDate)
            .Take(12) // Limita aos próximos 12 meses para o chart
            .Select(x => new EnaResultDto
            {
                TargetDate = x.TargetDate,
                ValueMwMed = x.ValueMwMed,
                ValuePercentageMlt = x.ValuePercentageMlt
            })
            .ToListAsync(cancellationToken);

        // Fallback Mock se a base estiver limpa
        if (!results.Any())
        {
            var fallback = new List<EnaResultDto>();
            for (int i = 0; i < 12; i++)
            {
                var rand = new Random();
                fallback.Add(new EnaResultDto
                {
                    TargetDate = DateTime.UtcNow.AddMonths(i),
                    ValuePercentageMlt = 50 + (rand.Next(30) + (request.OffsetDays * 5)), // Offset gera mudança visual no %MLT mock
                    ValueMwMed = 25000 + rand.Next(5000)
                });
            }
            return fallback;
        }

        return results;
    }
}
