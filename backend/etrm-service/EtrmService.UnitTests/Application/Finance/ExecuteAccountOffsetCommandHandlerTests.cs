using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Finance.Commands;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Moq;
using MockQueryable.Moq;
using Xunit;

namespace EtrmService.UnitTests.Application.Finance;

public class ExecuteAccountOffsetCommandHandlerTests
{
    private readonly Mock<IEtrmDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ExecuteAccountOffsetCommandHandler _handler;

    public ExecuteAccountOffsetCommandHandlerTests()
    {
        _contextMock = new Mock<IEtrmDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        
        _currentUserServiceMock.Setup(c => c.TenantId).Returns(Guid.NewGuid());
        
        _handler = new ExecuteAccountOffsetCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldOffsetAndGenerateResidualPayable_WhenPayableIsGreater()
    {
        // Arrange
        var counterpartyId = Guid.NewGuid();
        var refMonth = "2026-08";

        var payable1 = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Payable, 1000m, DateTime.UtcNow, refMonth);
        var payable2 = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Payable, 500m, DateTime.UtcNow, refMonth);
        var receivable = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Receivable, 1200m, DateTime.UtcNow, refMonth);

        var settlements = new List<FinancialSettlement> { payable1, payable2, receivable };
        var mockDbSet = settlements.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.FinancialSettlements).Returns(mockDbSet.Object);

        // Capture newly added settlement
        FinancialSettlement addedSettlement = null;
        _contextMock.Setup(c => c.FinancialSettlements.Add(It.IsAny<FinancialSettlement>()))
            .Callback<FinancialSettlement>(fs => addedSettlement = fs);

        var command = new ExecuteAccountOffsetCommand(counterpartyId, refMonth);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result); // offset group id

        // Original settlements should be marked as Offset
        Assert.Equal(FinancialSettlementStatus.Offset, payable1.Status);
        Assert.Equal(FinancialSettlementStatus.Offset, receivable.Status);
        Assert.Equal(result, payable1.OffsetGroupId);

        // Total Payable = 1500. Total Receivable = 1200. Residual should be 300 Payable.
        Assert.NotNull(addedSettlement);
        Assert.Equal(FinancialSettlementType.Payable, addedSettlement.Type);
        Assert.Equal(300m, addedSettlement.Amount);
        Assert.Equal(counterpartyId, addedSettlement.CounterpartyId);
        Assert.Equal(FinancialSettlementStatus.Open, addedSettlement.Status);
        
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task Handle_ShouldOffsetAndGenerateResidualReceivable_WhenReceivableIsGreater()
    {
        // Arrange
        var counterpartyId = Guid.NewGuid();
        var refMonth = "2026-08";

        var payable = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Payable, 500m, DateTime.UtcNow, refMonth);
        var receivable = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Receivable, 1200m, DateTime.UtcNow, refMonth);

        var settlements = new List<FinancialSettlement> { payable, receivable };
        var mockDbSet = settlements.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.FinancialSettlements).Returns(mockDbSet.Object);

        FinancialSettlement addedSettlement = null;
        _contextMock.Setup(c => c.FinancialSettlements.Add(It.IsAny<FinancialSettlement>()))
            .Callback<FinancialSettlement>(fs => addedSettlement = fs);

        var command = new ExecuteAccountOffsetCommand(counterpartyId, refMonth);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        
        // Total Payable = 500. Total Receivable = 1200. Residual should be 700 Receivable.
        Assert.NotNull(addedSettlement);
        Assert.Equal(FinancialSettlementType.Receivable, addedSettlement.Type);
        Assert.Equal(700m, addedSettlement.Amount);
    }
    
    [Fact]
    public async Task Handle_ShouldOffsetCompletely_WhenAmountsAreExact()
    {
        // Arrange
        var counterpartyId = Guid.NewGuid();
        var refMonth = "2026-08";

        var payable = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Payable, 1000m, DateTime.UtcNow, refMonth);
        var receivable = new FinancialSettlement(Guid.NewGuid(), counterpartyId, Guid.NewGuid(), FinancialSettlementType.Receivable, 1000m, DateTime.UtcNow, refMonth);

        var settlements = new List<FinancialSettlement> { payable, receivable };
        var mockDbSet = settlements.AsQueryable().BuildMockDbSet();

        _contextMock.Setup(c => c.FinancialSettlements).Returns(mockDbSet.Object);

        var command = new ExecuteAccountOffsetCommand(counterpartyId, refMonth);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(FinancialSettlementStatus.Offset, payable.Status);
        
        // Should NOT add any residual settlement
        _contextMock.Verify(c => c.FinancialSettlements.Add(It.IsAny<FinancialSettlement>()), Times.Never);
    }
}
