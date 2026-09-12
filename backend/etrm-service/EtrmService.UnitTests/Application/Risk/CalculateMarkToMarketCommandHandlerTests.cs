using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Risk.Commands;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EtrmService.UnitTests.Application.Risk
{
    public class CalculateMarkToMarketCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WhenPositionsGiven_CalculatesFallbackMtMSuccessfully()
        {
            // Arrange
            var httpClient = new HttpClient();
            var loggerMock = new Mock<ILogger<CalculateMarkToMarketCommandHandler>>();
            var handler = new CalculateMarkToMarketCommandHandler(httpClient, loggerMock.Object);

            var command = new CalculateMarkToMarketCommand
            {
                PortfolioId = Guid.NewGuid(),
                Positions = new List<ContractPositionDto>
                {
                    new ContractPositionDto
                    {
                        ContractId = Guid.NewGuid(),
                        Code = "CONT-001",
                        Submarket = "SE/CO",
                        VolumeMWm = 10.0m,
                        ContractPrice = 100.0m,
                        Type = "BUY"
                    },
                    new ContractPositionDto
                    {
                        ContractId = Guid.NewGuid(),
                        Code = "CONT-002",
                        Submarket = "SE/CO",
                        VolumeMWm = 15.0m,
                        ContractPrice = 60.0m,
                        Type = "SELL"
                    }
                }
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.ContractDetails.Count);
            Assert.True(result.TotalExposureValue > 0);
            Assert.NotNull(result.CalculatedAt);
        }
    }
}
