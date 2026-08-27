using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class CceeComparison
{
    public Guid Id { get; set; }
    public Guid? OperationId { get; set; } // Can be null if it's from CCEE but we don't have it
    public Guid? CounterpartyId { get; set; }
    public string CounterpartyCceeCode { get; set; }
    public DateTime Period { get; set; }
    public decimal BackOpsVolume { get; set; }
    public decimal CceeVolume { get; set; }
    public decimal Difference { get; set; }
    public CceeComparisonStatus Status { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public CceeComparison()
    {
    }

    public void UpdateStatus(CceeComparisonStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
