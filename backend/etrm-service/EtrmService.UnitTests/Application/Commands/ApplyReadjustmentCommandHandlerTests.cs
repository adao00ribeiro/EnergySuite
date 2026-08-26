using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Commands;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Shouldly;
using Xunit;

namespace EtrmService.UnitTests.Application.Commands;

public class ApplyReadjustmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidReadjustment_ShouldCreateAmendmentAndUpdateContract()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var contract = new Contract(
            "Acme Corp",
            ContractType.Purchase,
            EnergySubmarket.SE_CO,
            10m,
            100m, // Price is 100
            new DateTime(2025, 1, 1),
            new DateTime(2030, 12, 31),
            null, null, tenantId, PriceIndexType.IPCA, 0.1m
        );

        // A reflection hack to set the Id (which has private setter) for testing purposes
        typeof(Contract).GetProperty("Id")?.SetValue(contract, contractId);

        var contracts = new List<Contract> { contract };
        var mockDbSet = contracts.AsQueryable().BuildMockDbSet();

        var mockContext = new Mock<IEtrmDbContext>();
        mockContext.Setup(c => c.Contracts).Returns(mockDbSet.Object);

        var command = new ApplyReadjustmentCommand(contractId, 110m, "Reajuste IPCA 10%", new DateTime(2026, 1, 1));
        var handler = new ApplyReadjustmentCommandHandler(mockContext.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBeTrue();
        contract.Price.ShouldBe(110m);
        contract.Version.ShouldBe(2);
        contract.Amendments.Count.ShouldBe(1);

        var amendment = contract.Amendments.First();
        amendment.ShouldNotBeNull();
        amendment.PreviousPrice.ShouldBe(100m);
        amendment.NewPrice.ShouldBe(110m);
        amendment.Description.ShouldBe("Reajuste IPCA 10%");
        amendment.Version.ShouldBe(2);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
// Note: We need a dummy MockCurrentUserService and EtrmDbContext to use here.
// I will just use EtrmDbContext directly. Wait, EtrmDbContext is in Infrastructure.
// We'd have to reference EtrmService.Infrastructure.
