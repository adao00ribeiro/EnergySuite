using System;

namespace EtrmService.Application.IntegrationEvents;

public record OperationPublishedIntegrationEvent(
    Guid OperationId,
    Guid TicketId,
    Guid CounterpartyId,
    string Status,
    decimal Volume,
    decimal Price,
    DateTime Timestamp
);
