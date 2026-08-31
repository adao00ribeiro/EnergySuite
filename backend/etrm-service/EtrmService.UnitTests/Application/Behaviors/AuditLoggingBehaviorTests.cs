using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Behaviors;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EtrmService.UnitTests.Application.Behaviors;

public class AuditLoggingBehaviorTests
{
    private readonly Guid _tenantId;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AuditLoggingBehavior<SampleCommand, string> _behavior;

    public sealed record SampleCommand(string Value) : IRequest<string>
    {
        public Guid Id { get; init; } = Guid.NewGuid();
    }

    public AuditLoggingBehaviorTests()
    {
        _tenantId = Guid.NewGuid();

        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _currentUserServiceMock.Setup(c => c.TenantId).Returns(_tenantId);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("test-user");

        var loggerMock = new Mock<ILogger<AuditLoggingBehavior<SampleCommand, string>>>();

        _behavior = new AuditLoggingBehavior<SampleCommand, string>(
            loggerMock.Object,
            _auditLogRepositoryMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCommandSucceeds_PersistsAuditLogAndSaves()
    {
        // Arrange
        var command = new SampleCommand("value-1");
        AuditLog? captured = null;
        _auditLogRepositoryMock
            .Setup(d => d.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _behavior.Handle(command, _ => Task.FromResult("ok"), CancellationToken.None);

        // Assert
        Assert.Equal("ok", result);

        _auditLogRepositoryMock.Verify(d => d.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);

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
        _auditLogRepositoryMock
            .Setup(d => d.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Callback<AuditLog, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        // Act
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _behavior.Handle(command, _ => throw new InvalidOperationException("boom"), CancellationToken.None));

        // Assert
        Assert.Equal("boom", ex.Message);

        _auditLogRepositoryMock.Verify(d => d.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);

        Assert.NotNull(captured);
        Assert.Equal(nameof(SampleCommand), captured!.EntityName);
        Assert.Equal(command.Id.ToString(), captured.EntityId);
        Assert.Equal("Failed: InvalidOperationException", captured.Action);
        Assert.Equal("test-user", captured.ChangedBy);
        Assert.Equal(_tenantId, captured.TenantId);
        Assert.Contains(command.Id.ToString(), captured.ChangesJson);
    }
}