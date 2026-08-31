using System;
using EtrmService.Domain.Entities;
using MediatR;

namespace EtrmService.Application.Pluvia.Queries;

public record GetLatestPrecipitationScenarioQuery : IRequest<PrecipitationScenario?>;
