using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Pluvia.DTOs;
using EtrmService.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Pluvia.Queries;

public class GetForecastMetadataQueryHandler : IRequestHandler<GetForecastMetadataQuery, IEnumerable<ForecastMetadataDto>>
{
    private readonly IEtrmDbContext _context;
    private readonly ILogger<GetForecastMetadataQueryHandler> _logger;

    public GetForecastMetadataQueryHandler(IEtrmDbContext context, ILogger<GetForecastMetadataQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ForecastMetadataDto>> Handle(GetForecastMetadataQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching available forecast metadata from Lakehouse index");

        var metadata = await _context.ForecastMetadatas
            .AsNoTracking()
            .OrderByDescending(m => m.ReferenceDate)
            .Select(m => new ForecastMetadataDto
            {
                Id = m.Id,
                ModelName = m.ModelName,
                ReferenceDate = m.ReferenceDate,
                Resolution = m.Resolution,
                EnsembleMembers = m.EnsembleMembers,
                LakehousePath = m.LakehousePath,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Se estiver vazio (só para efeitos de demonstração na Sprint 2 onde Kafka ainda não insere),
        // podemos retornar uma lista default na controller ou aqui.
        // Optaremos por retornar o que está no banco (AsNoTracking).

        return metadata;
    }
}
