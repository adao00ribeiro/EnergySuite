using System.Collections.Generic;
using MediatR;
using EtrmService.Application.Operations.DTOs;

namespace EtrmService.Application.Operations.Queries;

public class GetOperationsQuery : IRequest<List<OperationDto>>
{
}
