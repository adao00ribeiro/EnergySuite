using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Finance.DTOs;

namespace EtrmService.Application.Finance.Queries;

public record GetFinancialDashboardQuery : IRequest<FinancialDashboardDto>;
