using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class PriceIndexValue
{
    public Guid Id { get; private set; }
    public PriceIndexType IndexType { get; private set; }
    
    public string ReferenceMonth { get; private set; } = string.Empty;
    public decimal MonthlyRate { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected PriceIndexValue() { } // EF Core

    public PriceIndexValue(PriceIndexType indexType, string referenceMonth, decimal monthlyRate)
    {
        Id = Guid.NewGuid();
        IndexType = indexType;
        ReferenceMonth = referenceMonth;
        MonthlyRate = monthlyRate;
        CreatedAt = DateTime.UtcNow;
    }
}
