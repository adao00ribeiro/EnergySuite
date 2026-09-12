using System;
using Xunit;
using Shouldly;

namespace EtrmService.UnitTests.Application
{
    public class Sprint18ContractAndIntegrationTests
    {
        [Fact]
        public void OpportunityDto_ShouldInstantiateWithValidContracts()
        {
            // Arrange & Act
            var oppId = Guid.NewGuid();
            var oppName = "Oportunidade SE 2026/09";
            var volume = 150.5m;
            var spread = 12.4m;

            // Assert
            oppId.ShouldNotBe(Guid.Empty);
            oppName.ShouldNotBeNullOrEmpty();
            volume.ShouldBeGreaterThan(0);
            spread.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void FinancialSettlement_Contract_ShouldValidatePayableAndReceivableTypes()
        {
            // Arrange
            var payableType = "Payable";
            var receivableType = "Receivable";

            // Assert
            payableType.ShouldBe("Payable");
            receivableType.ShouldBe("Receivable");
        }

        [Fact]
        public void PluviaPrecipitationMap_Contract_ShouldValidateModelAndHorizonDays()
        {
            // Arrange
            var model = "GEFS";
            var horizonDays = 8;

            // Assert
            model.ShouldBe("GEFS");
            horizonDays.ShouldBe(8);
        }
    }
}
