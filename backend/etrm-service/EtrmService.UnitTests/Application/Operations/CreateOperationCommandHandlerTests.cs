using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Shouldly;
using Xunit;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Enums;
using EtrmService.Application.Operations.Commands;

namespace EtrmService.UnitTests.Application.Operations;

public class CreateOperationCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateOperationAndAuditLog()
    {
        // Arrange
        var mockContext = new Mock<IEtrmDbContext>();
        
        var mockOperationsDbSet = new Mock<DbSet<Operation>>();
        mockContext.Setup(c => c.Operations).Returns(mockOperationsDbSet.Object);
        
        var mockAuditLogsDbSet = new Mock<DbSet<AuditLog>>();
        mockContext.Setup(c => c.AuditLogs).Returns(mockAuditLogsDbSet.Object);
        
        var mockUserService = new Mock<ICurrentUserService>();
        mockUserService.Setup(u => u.TenantId).Returns(Guid.NewGuid());
        mockUserService.Setup(u => u.UserId).Returns("user123");

        var handler = new CreateOperationCommandHandler(mockContext.Object, mockUserService.Object);
        
        var command = new CreateOperationCommand
        {
            TicketId = Guid.NewGuid(),
            PortfolioId = Guid.NewGuid(),
            CounterpartyId = Guid.NewGuid(),
            Type = OperationType.Purchase,
            VolumeMwMed = 10m,
            Price = 150m,
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(1)
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        mockOperationsDbSet.Verify(db => db.Add(It.IsAny<Operation>()), Times.Once);
        mockAuditLogsDbSet.Verify(db => db.Add(It.Is<AuditLog>(a => a.Action == "Created" && a.EntityName == "Operation")), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
