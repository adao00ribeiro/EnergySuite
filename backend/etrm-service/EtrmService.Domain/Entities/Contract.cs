using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Domain.Entities;

public class Contract
{
    public Guid Id { get; private set; }
    public string CounterpartyName { get; private set; } = string.Empty;
    public ContractType Type { get; private set; }
    public EnergySubmarket Submarket { get; private set; }
    
    // Volume de energia negociada em Megawatt Médio (MWm)
    public decimal VolumeMwMed { get; private set; }
    
    // Preço em Reais por Megawatt Hora (R$/MWh)
    public decimal Price { get; private set; }
    
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Construtor vazio necessário pelo Entity Framework
    protected Contract() { }

    public Contract(
        string counterpartyName, 
        ContractType type, 
        EnergySubmarket submarket, 
        decimal volumeMwMed, 
        decimal price, 
        DateTime startDate, 
        DateTime endDate)
    {
        Id = Guid.NewGuid();
        CounterpartyName = counterpartyName;
        Type = type;
        Submarket = submarket;
        VolumeMwMed = volumeMwMed;
        Price = price;
        StartDate = startDate;
        EndDate = endDate;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("O preço não pode ser negativo.");

        Price = newPrice;
        UpdatedAt = DateTime.UtcNow;
    }
}
