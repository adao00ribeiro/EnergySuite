using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Queries;
using EtrmService.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Operations.Commands;

public class SimulationResultDto
{
    public decimal PreviousVolumeMwm { get; set; }
    public decimal NewVolumeMwm { get; set; }
    public decimal VolumeDelta => NewVolumeMwm - PreviousVolumeMwm;

    public decimal PreviousEstimatedResult { get; set; }
    public decimal NewEstimatedResult { get; set; }
    public decimal FinancialDelta => NewEstimatedResult - PreviousEstimatedResult;

    public CopilotInsightDto CopilotAnalysis { get; set; } = new();
}

public class SimulateOperationCommand : IRequest<SimulationResultDto>
{
    public Guid OpportunityId { get; set; }
    public Guid PortfolioId { get; set; }
    public decimal? VolumeMwm { get; set; }
    public decimal? Price { get; set; }
    public string? TargetMonth { get; set; }
}

public class SimulateOperationCommandHandler : IRequestHandler<SimulateOperationCommand, SimulationResultDto>
{
    private readonly IMediator _mediator;
    private readonly ITradingCopilotService _copilotService;
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SimulateOperationCommandHandler(
        IMediator mediator,
        ITradingCopilotService copilotService,
        IEtrmDbContext context,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _copilotService = copilotService;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SimulationResultDto> Handle(SimulateOperationCommand request, CancellationToken cancellationToken)
    {
        var portfolioId = request.PortfolioId;
        if (portfolioId == Guid.Empty)
        {
            portfolioId = await _context.Portfolios
                .Where(p => p.TenantId == _currentUserService.TenantId)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var currentYear = DateTime.UtcNow.Year;
        var before = await _mediator.Send(new GetPortfolioPositionQuery(portfolioId, _currentUserService.TenantId, currentYear), cancellationToken);

        var shouldApply = portfolioId != Guid.Empty && request.VolumeMwm.HasValue;
        var opVolume = shouldApply ? request.VolumeMwm!.Value : 0m;
        var opPrice = request.Price ?? 0m;

        var previousVolume = portfolioId != Guid.Empty ? before.NetPositionMwMed : 0m;
        var previousResult = portfolioId != Guid.Empty ? before.EstimatedResult : 0m;
        var newVolume = previousVolume + opVolume;
        var newResult = previousResult - (opVolume * opPrice);

        var result = new SimulationResultDto
        {
            PreviousVolumeMwm = previousVolume,
            NewVolumeMwm = newVolume,
            PreviousEstimatedResult = previousResult,
            NewEstimatedResult = newResult
        };

        result.CopilotAnalysis = await _copilotService.AnalyzeSimulationAsync(result.VolumeDelta, result.FinancialDelta);

        return result;
    }
}
