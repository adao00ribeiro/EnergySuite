using System;
using System.Collections.Generic;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class Contract
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string CounterpartyName { get; private set; } = string.Empty;
    public ContractType Type { get; private set; }
    public EnergySubmarket Submarket { get; private set; }
    
    // Volume de energia negociada em Megawatt Médio (MWm)
    public decimal VolumeMwMed { get; private set; }
    
    // Preço em Reais por Megawatt Hora (R$/MWh)
    public decimal Price { get; private set; }
    
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    // Propriedades Específicas de Derivativos
    public decimal? StrikePrice { get; private set; }
    public decimal? OptionPremium { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Sprint 3: Reajustes e Aditivos
    public int Version { get; private set; } = 1;
    public PriceIndexType PriceIndexType { get; private set; }
    public decimal FlexibilityMargin { get; private set; }
    
    private readonly List<ContractAmendment> _amendments = new();
    public IReadOnlyCollection<ContractAmendment> Amendments => _amendments.AsReadOnly();

    // Construtor vazio necessário pelo Entity Framework
    protected Contract() { }

    public Contract(
        string counterpartyName, 
        ContractType type, 
        EnergySubmarket submarket, 
        decimal volumeMwMed, 
        decimal price, 
        DateTime startDate, 
        DateTime endDate,
        decimal? strikePrice = null,
        decimal? optionPremium = null,
        Guid tenantId = default,
        PriceIndexType priceIndexType = PriceIndexType.None,
        decimal flexibilityMargin = 0m)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId == default ? Guid.Parse("00000000-0000-0000-0000-000000000001") : tenantId;
        CounterpartyName = counterpartyName;
        Type = type;
        Submarket = submarket;
        VolumeMwMed = volumeMwMed;
        Price = price;
        StartDate = DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        EndDate = DateTime.SpecifyKind(endDate, DateTimeKind.Utc);
        StrikePrice = strikePrice;
        OptionPremium = optionPremium;
        PriceIndexType = priceIndexType;
        FlexibilityMargin = flexibilityMargin;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
    }

    public void ApplyReadjustment(decimal newPrice, string description, DateTime effectiveDate)
    {
        if (newPrice <= 0)
            throw new ArgumentException("O novo preço deve ser maior que zero.");

        var amendment = new ContractAmendment(
            Id,
            Version + 1,
            description,
            effectiveDate,
            Price,
            newPrice,
            VolumeMwMed,
            VolumeMwMed
        );

        _amendments.Add(amendment);
        
        Price = newPrice;
        Version++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("O preço não pode ser negativo.");

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
