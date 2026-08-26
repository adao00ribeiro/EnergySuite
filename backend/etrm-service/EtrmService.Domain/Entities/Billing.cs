using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class Billing
{
    public Guid Id { get; private set; }
    public Guid OperationId { get; private set; }
    
    // e.g., "2026-08"
    public string ReferenceMonth { get; private set; } = string.Empty;
    
    public decimal CalculatedVolume { get; private set; }
    public decimal AppliedPrice { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal TaxesAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    
    public BillingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    public Operation Operation { get; private set; } = null!;

    protected Billing() { }

    public Billing(Guid operationId, string referenceMonth, decimal calculatedVolume, decimal appliedPrice, decimal taxesAmount)
    {
        Id = Guid.NewGuid();
        OperationId = operationId;
        ReferenceMonth = referenceMonth;
        CalculatedVolume = calculatedVolume;
        AppliedPrice = appliedPrice;
        TaxesAmount = taxesAmount;
        GrossAmount = calculatedVolume * appliedPrice;
        NetAmount = GrossAmount + taxesAmount; // taxes can be negative or positive depending on domain
        Status = BillingStatus.Draft;
        CreatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != BillingStatus.Draft)
            throw new InvalidOperationException("Only Draft billings can be approved.");
            
        Status = BillingStatus.Approved;
    }

    public void MarkAsInvoiced()
    {
        if (Status != BillingStatus.Approved)
            throw new InvalidOperationException("Only Approved billings can be invoiced.");
            
        Status = BillingStatus.Invoiced;
    }
}
