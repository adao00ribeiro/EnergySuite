using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Behaviors;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EtrmService.UnitTests.Application.Behaviors;

public class AuditLoggingBehaviorTests
{
    private readonly Guid _tenantId;
    private readonly Mock<IEtrmDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<DbSet<AuditLog>> _auditLogsDbSetMock;
    private readonly AuditLoggingBehavior<SampleCommand, string> _behavior;

    public sealed record SampleCommand(string Value) : IRequest<string>
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }

    public AuditLoggingBehaviorTests()
    {
        _tenantId = Guid.NewGuid();

        _contextMock = new Mock<IEtrmDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _auditLogsDbSetMock = new Mock<DbSet<AuditLog>>();

        _contextMock.Setup(c => c.AuditLogs).Returns(_auditLogsDbSetMock.Object);
        _currentUserServiceMock.Setup(c => c.TenantId).Returns(_tenantId);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("test-user");

        var loggerMock = new Mock<ILogger<AuditLoggingBehavior<SampleCommand, string>>>();

        _behavior = new AuditLoggingBehavior<SampleCommand, string>(
            loggerMock.Object,
            _contextMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCommandSucceeds_PersistsAuditLogAndSaves()
    {
        // Arrange
        var command = new SampleCommand("value-1");
        AuditLog? captured = null;
        _auditLogsDbSetMock
            .Setup(d => d.Add(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(a => captured = a);

        // Act
        var result = await _behavior.Handle(command, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);

        _auditLogsDbSetMock.Verify(d => d.Add(It.IsAny<AuditLog>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(captured);
        Assert.Equal(nameof(SampleCommand), captured!.EntityName);
        Assert.Equal(command.Id.ToString(), captured.EntityId);
        Assert.Equal("Executed", captured.Action);
        Assert.Equal("test-user", captured.ChangedBy);
        Assert.Equal(_tenantId, captured.TenantId);
        Assert.Contains(command.Id.ToString(), captured.ChangesJson);
    }

    [Fact]
    public async Task Handle_WhenCommandThrows_PersistsFailureAuditLogAndPropagatesException()
    {
        // Arrange
        var command = new SampleCommand("value-2");
        AuditLog? captured = null;
        _auditLogsDbSetMock
            .Setup(d => d.Add(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(a => captured = a);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(command, _ => throw new InvalidOperationException("boom"), CancellationToken.None));

        // Assert
        Assert.Equal("boom", ex.Message);

        _auditLogsDbSetMock.Verify(d => d.Add(It.IsAny<AuditLog>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(captured);
        Assert.Equal(nameof(SampleCommand), captured!.EntityName);
        Assert.Equal(command.Id.ToString(), captured.EntityId);
        Assert.Equal("Failed: InvalidOperationException", captured.Action);
        Assert.Equal("test-user", captured.ChangedBy);
        Assert.Equal(_tenantId, captured.TenantId);
        Assert.Contains(command.Id.ToString(), captured.ChangesJson);
    }
}