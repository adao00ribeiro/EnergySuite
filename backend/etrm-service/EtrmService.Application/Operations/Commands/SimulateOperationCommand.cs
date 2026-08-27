using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Services;

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
}

public class SimulateOperationCommandHandler : IRequestHandler<SimulateOperationCommand, SimulationResultDto>
{
    private readonly ITradingCopilotService _copilotService;

    public SimulateOperationCommandHandler(ITradingCopilotService copilotService)
    {
        _copilotService = copilotService;
    }

    public async Task<SimulationResultDto> Handle(SimulateOperationCommand request, CancellationToken cancellationToken)
    {
        // Mocking the "Before" state of the portfolio
        var prevVolume = 30.5m; // Net position from Sprint 1
        var prevResult = 450000.00m; 

        // Mocking the operation impact (e.g. buying 15.5 MWm at a cost of 12k)
        // In a real scenario, we would load the Opportunity details from the DB and calculate
        var opVolume = 15.5m; 
        var opSpread = -12000.00m;

        var result = new SimulationResultDto
        {
            PreviousVolumeMwm = prevVolume,
            NewVolumeMwm = prevVolume + opVolume,
            PreviousEstimatedResult = prevResult,
            NewEstimatedResult = prevResult + opSpread
        };

        result.CopilotAnalysis = await _copilotService.AnalyzeSimulationAsync(result.VolumeDelta, result.FinancialDelta);

        return result;
    }
}
