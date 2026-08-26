using System.Collections.Generic;
using MediatR;

namespace EtrmService.Application.Pluvia.Queries;

public class GetEnaResultsQuery : IRequest<IEnumerable<EnaResultDto>>
{
    public string? Submarket { get; set; }
    public int OffsetDays { get; set; } = 0; // Para evolucao de previsao (hoje, ontem, etc.)
}
