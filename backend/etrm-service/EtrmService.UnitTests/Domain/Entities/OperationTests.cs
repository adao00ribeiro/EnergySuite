using System;
using Shouldly;
using Xunit;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;

namespace EtrmService.UnitTests.Domain.Entities;

public class OperationTests
{
    [Fact]
    public void Operation_WhenCreated_ShouldHaveDraftState()
    {
        // Arrange
        var ticketId = Guid.NewGuid();
        var portfolioId = Guid.NewGuid();
        var counterpartyId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        
        // Act
        var operation = new Operation(ticketId, portfolioId, counterpartyId, OperationType.Purchase, 10.5m, 120m, DateTime.Now, DateTime.Now.AddDays(30), tenantId);

        // Assert
        operation.State.ShouldBe(OperationState.Draft);
        operation.VolumeMwMed.ShouldBe(10.5m);
        operation.Price.ShouldBe(120m);
    }

    [Fact]
    public void Operation_ChangeState_ShouldUpdateStateCorrectly()
    {
        // Arrange
        var operation = new Operation(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), OperationType.Sale, 15m, 100m, DateTime.Now, DateTime.Now.AddDays(30), Guid.NewGuid());
        
        // Act
        operation.ChangeState(OperationState.Validation);

        // Assert
        operation.State.ShouldBe(OperationState.Validation);
    }
}
