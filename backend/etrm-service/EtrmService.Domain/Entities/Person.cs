using System;
using EtrmService.Domain.ValueObjects;

namespace EtrmService.Domain.Entities;

public class Person
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string Cpf { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? AdditionalCharacteristics { get; private set; }
    
    public Address Address { get; private set; } = null!;
    public ContactInfo ContactInfo { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    protected Person() { }

    public Person(string cpf, string name, Guid tenantId = default)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId == default ? Guid.Parse("00000000-0000-0000-0000-000000000001") : tenantId;
        Cpf = cpf;
        Name = name;
        CreatedAt = DateTime.UtcNow;
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
}
