using System;
using EtrmService.Domain.Enums;
using EtrmService.Domain.ValueObjects;

namespace EtrmService.Domain.Entities;

public class Company
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Cnpj { get; private set; } = string.Empty;
    public string CorporateName { get; private set; } = string.Empty;
    public string TradeName { get; private set; } = string.Empty;
    public string? StateRegistration { get; private set; }
    public string? EconomicActivity { get; private set; }
    
    public CompanyCategory Category { get; private set; }
    
    // CCEE Data
    public string? CceeProfile { get; private set; }
    public string? CceeCode { get; private set; }
    public string? CceeAcronym { get; private set; }
    public CceeClass? Class { get; private set; }

    public Address Address { get; private set; } = null!;
    public ContactInfo ContactInfo { get; private set; } = null!;

    public Guid? EconomicGroupId { get; private set; }
    public EconomicGroup? EconomicGroup { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Company() { }

    public Company(string cnpj, string corporateName, string tradeName, CompanyCategory category, Guid tenantId = default)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId == default ? Guid.Parse("00000000-0000-0000-0000-000000000001") : tenantId;
        Cnpj = cnpj;
        CorporateName = corporateName;
        TradeName = tradeName;
        Category = category;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateCceeData(string? profile, string? code, string? acronym, CceeClass? cceeClass)
    {
        CceeProfile = profile;
        CceeCode = code;
        CceeAcronym = acronym;
        Class = cceeClass;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignToEconomicGroup(Guid groupId)
    {
        EconomicGroupId = groupId;
        UpdatedAt = DateTime.UtcNow;
    }
}
