using System.Collections.Generic;
using EtrmService.Application.Pluvia.DTOs;
using MediatR;

namespace EtrmService.Application.Pluvia.Queries;

public class GetPrecipitationScenariosQuery : IRequest<IEnumerable<PrecipitationScenarioDto>>
{
}
