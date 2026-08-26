using System;

namespace EtrmService.Application.Pluvia.Queries;

public class ModelExecutionDto
{
    public Guid Id { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Accuracy { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
