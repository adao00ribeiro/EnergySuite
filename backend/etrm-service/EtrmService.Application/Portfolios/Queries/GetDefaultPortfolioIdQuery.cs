using System;
using MediatR;

namespace EtrmService.Application.Portfolios.Queries;

public record GetDefaultPortfolioIdQuery(Guid? ExplicitPortfolioId, Guid TenantId) : IRequest<Guid>;
