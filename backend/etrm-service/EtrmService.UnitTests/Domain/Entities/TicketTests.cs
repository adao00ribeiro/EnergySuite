using System;
using Shouldly;
using Xunit;
using EtrmService.Domain.Entities;

namespace EtrmService.UnitTests.Domain.Entities;

public class TicketTests
{
    [Fact]
    public void Ticket_WhenCreated_ShouldSetProperties()
    {
        // Arrange
        var referenceNumber = "TKT-2023-001";
        var tenantId = Guid.NewGuid();

        // Act
        var ticket = new Ticket(referenceNumber, tenantId);

        // Assert
        ticket.ReferenceNumber.ShouldBe(referenceNumber);
        ticket.TenantId.ShouldBe(tenantId);
        ticket.Operations.ShouldBeEmpty();
        ticket.Id.ShouldNotBe(Guid.Empty);
    }
}
