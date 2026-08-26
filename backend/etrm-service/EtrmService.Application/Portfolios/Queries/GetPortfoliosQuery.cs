using System.Collections.Generic;
using MediatR;
using EtrmService.Application.Portfolios.DTOs;

namespace EtrmService.Application.Portfolios.Queries;

public class GetPortfoliosQuery : IRequest<List<PortfolioDto>>
{
}
