using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class FinancialSettlement
{
    public Guid Id { get; private set; }
    
    // Nullable if the settlement is an offset residual and not tied directly to one billing
    public Guid? BillingId { get; private set; }
    
    public Guid CounterpartyId { get; private set; }
    public Guid TenantId { get; private set; }
    
    public FinancialSettlementType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public string ReferenceMonth { get; private set; } = string.Empty;
    
    public FinancialSettlementStatus Status { get; private set; }
    
    // Used to group settlements that were offset against each other
    public Guid? OffsetGroupId { get; private set; }
    
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Billing? Billing { get; private set; }
    public Company Counterparty { get; private set; } = null!;

    protected FinancialSettlement() { }

    public FinancialSettlement(Guid? billingId, Guid counterpartyId, Guid tenantId, FinancialSettlementType type, decimal amount, DateTime dueDate, string referenceMonth)
    {
        Id = Guid.NewGuid();
        BillingId = billingId;
        CounterpartyId = counterpartyId;
        TenantId = tenantId;
        Type = type;
        Amount = amount;
        DueDate = dueDate;
        ReferenceMonth = referenceMonth;
        Status = FinancialSettlementStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsOffset(Guid offsetGroupId)
    {
        if (Status != FinancialSettlementStatus.Open)
            throw new InvalidOperationException("Only Open settlements can be offset.");
            
        Status = FinancialSettlementStatus.Offset;
        OffsetGroupId = offsetGroupId;
    }

    public void MarkAsSettled()
    {
        if (Status != FinancialSettlementStatus.Open)
            throw new InvalidOperationException("Only Open settlements can be settled.");
            
        Status = FinancialSettlementStatus.Settled;
    }
}
