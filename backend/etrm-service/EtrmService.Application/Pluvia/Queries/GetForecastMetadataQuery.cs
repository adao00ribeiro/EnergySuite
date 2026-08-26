using System.Collections.Generic;
using EtrmService.Application.Pluvia.DTOs;
using MediatR;

namespace EtrmService.Application.Pluvia.Queries;

public record GetForecastMetadataQuery : IRequest<IEnumerable<ForecastMetadataDto>>;
