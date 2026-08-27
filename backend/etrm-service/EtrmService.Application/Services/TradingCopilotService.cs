using System;
using System.Threading.Tasks;

namespace EtrmService.Application.Services;

public class CopilotInsightDto
{
    public string SummaryText { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty; // "Approve", "Reject", "Warning"
}

public interface ITradingCopilotService
{
    Task<CopilotInsightDto> AnalyzeSimulationAsync(decimal volumeDelta, decimal financialDelta);
}

public class TradingCopilotService : ITradingCopilotService
{
    public async Task<CopilotInsightDto> AnalyzeSimulationAsync(decimal volumeDelta, decimal financialDelta)
    {
        await Task.Delay(100); // Simulate AI generation processing

        var insight = new CopilotInsightDto();

        if (financialDelta > 0)
        {
            insight.SummaryText = $"A simulação projeta um aumento no resultado financeiro de R$ {financialDelta:N2}. " +
                                  $"O volume movimentado é de {volumeDelta} MWm. Esta operação melhora a rentabilidade da carteira.";
            insight.Recommendation = "Approve";
        }
        else if (financialDelta < 0 && volumeDelta > 0)
        {
            insight.SummaryText = $"Esta é uma operação de hedge/cobertura. A aquisição de {volumeDelta} MWm " +
                                  $"terá um custo (redução no resultado estimado) de R$ {Math.Abs(financialDelta):N2}. " +
                                  $"O déficit no submercado será reduzido significativamente.";
            insight.Recommendation = "Approve"; // Hedge cost is expected
        }
        else
        {
            insight.SummaryText = $"Atenção: A operação não apresenta benefícios claros no modelo heurístico atual " +
                                  $"(Impacto Financeiro: R$ {financialDelta:N2}, Volume: {volumeDelta} MWm). " +
                                  $"Pode expor o portfólio a riscos desnecessários.";
            insight.Recommendation = "Warning";
        }

        return insight;
    }
}
