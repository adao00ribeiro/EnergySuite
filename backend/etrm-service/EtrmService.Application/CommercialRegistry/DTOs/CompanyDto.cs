using System;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.CommercialRegistry.DTOs;

public class CompanyDto
{
    public Guid Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string CorporateName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Address
    public string? City { get; set; }
    public string? State { get; set; }
    
    // CCEE
    public string? CceeCode { get; set; }
    public string? CceeProfile { get; set; }
}
