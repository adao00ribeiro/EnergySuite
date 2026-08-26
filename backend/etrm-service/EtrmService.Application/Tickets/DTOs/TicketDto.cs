using System;

namespace EtrmService.Application.Tickets.DTOs;

public class TicketDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
