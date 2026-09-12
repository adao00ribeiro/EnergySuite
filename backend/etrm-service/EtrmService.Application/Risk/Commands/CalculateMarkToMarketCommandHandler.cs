using System;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EtrmService.Application.Risk.Commands
{
    public class CalculateMarkToMarketCommandHandler : IRequestHandler<CalculateMarkToMarketCommand, MarkToMarketResultDto>
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CalculateMarkToMarketCommandHandler> _logger;

        public CalculateMarkToMarketCommandHandler(
            HttpClient httpClient,
            ILogger<CalculateMarkToMarketCommandHandler> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<MarkToMarketResultDto> Handle(CalculateMarkToMarketCommand request, CancellationToken cancellationToken)
        {
            var riskServiceUrl = Environment.GetEnvironmentVariable("RISK_SERVICE_URL") ?? "http://localhost:8000";
            var endpoint = $"{riskServiceUrl}/api/v1/risk/mark-to-market";

            try
            {
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }),
                    Encoding.UTF8,
                    "application/json"
                );

                _logger.LogInformation("Calling Risk Service MtM endpoint at {Endpoint} for {Count} positions", endpoint, request.Positions.Count);

                var response = await _httpClient.PostAsync(endpoint, jsonContent, cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var result = JsonSerializer.Deserialize<MarkToMarketResultDto>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null) return result;
                }

                _logger.LogWarning("Risk service returned status code {StatusCode}. Falling back to internal MtM calculation.", response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Risk Service MtM endpoint. Performing fallback calculation.");
            }

            // Fallback calculation in C# if Python service is unreachable
            return PerformFallbackMtM(request);
        }

        private MarkToMarketResultDto PerformFallbackMtM(CalculateMarkToMarketCommand request)
        {
            var result = new MarkToMarketResultDto
            {
                CalculatedAt = DateTime.UtcNow
            };

            decimal totalMtM = 0m;
            decimal totalExp = 0m;
            decimal defaultMarketPrice = 75.50m; // PLD médio estimado

            foreach (var pos in request.Positions)
            {
                var priceDiff = pos.Type.ToUpper() == "BUY"
                    ? defaultMarketPrice - pos.ContractPrice
                    : pos.ContractPrice - defaultMarketPrice;

                var mtmValue = pos.VolumeMWm * 720 * priceDiff; // 720h padrão mês
                var exposure = pos.VolumeMWm * 720 * defaultMarketPrice;

                totalMtM += mtmValue;
                totalExp += exposure;

                result.ContractDetails.Add(new ContractMtMDetailDto
                {
                    ContractId = pos.ContractId,
                    Code = string.IsNullOrEmpty(pos.Code) ? pos.ContractId.ToString().Substring(0, 8) : pos.Code,
                    VolumeMWm = pos.VolumeMWm,
                    ContractPrice = pos.ContractPrice,
                    CurrentMarketPrice = defaultMarketPrice,
                    MtMValue = mtmValue,
                    MtMPercent = pos.ContractPrice > 0 ? (priceDiff / pos.ContractPrice) * 100 : 0
                });
            }

            result.TotalMtMValue = totalMtM;
            result.TotalExposureValue = totalExp;
            return result;
        }
    }
}
