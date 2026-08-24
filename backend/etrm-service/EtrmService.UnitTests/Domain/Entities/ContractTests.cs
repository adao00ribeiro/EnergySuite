using System;
using FluentAssertions;
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
        contract.Should().NotBeNull();
        contract.Id.Should().NotBeEmpty();
        contract.CounterpartyName.Should().Be(counterpartyName);
        contract.Type.Should().Be(type);
        contract.Submarket.Should().Be(submarket);
        contract.VolumeMwMed.Should().Be(volumeMwMed);
        contract.Price.Should().Be(price);
        contract.StartDate.Should().Be(startDate);
        contract.EndDate.Should().Be(endDate);
    }
}
