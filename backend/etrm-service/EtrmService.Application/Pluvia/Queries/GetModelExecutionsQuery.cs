using System.Collections.Generic;
using MediatR;

namespace EtrmService.Application.Pluvia.Queries;

public class GetModelExecutionsQuery : IRequest<IEnumerable<ModelExecutionDto>>
{
}
