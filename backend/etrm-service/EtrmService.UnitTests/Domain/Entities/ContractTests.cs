using System;
using Shouldly;
using Xunit;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Domain.Entities;

public class ContractTests
{
    [Fact]
    public void Constructor_Should_Create_Contract_With_Valid_Parameters()
    {
        // Arrange
        var counterpartyName = "Energy Corp Ltda";
        var type = ContractType.Purchase;
        var submarket = EnergySubmarket.SE_CO;
        var volumeMwMed = 10.5m;
        var price = 250.00m;
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);

        // Act
        var contract = new Contract(counterpartyName, type, submarket, volumeMwMed, price, startDate, endDate);

        // Assert
        contract.ShouldNotBeNull();
        contract.Id.ShouldNotBe(Guid.Empty);
        contract.CounterpartyName.ShouldBe(counterpartyName);
        contract.Type.ShouldBe(type);
        contract.Submarket.ShouldBe(submarket);
        contract.VolumeMwMed.ShouldBe(volumeMwMed);
        contract.Price.ShouldBe(price);
        contract.StartDate.ShouldBe(startDate);
        contract.EndDate.ShouldBe(endDate);
    }
}
