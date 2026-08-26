using System;
using MediatR;
using EtrmService.Domain.Enums;

namespace EtrmService.Application.CommercialRegistry.Commands;

public class CreateCompanyCommand : IRequest<Guid>
{
    public string Cnpj { get; set; } = string.Empty;
    public string CorporateName { get; set; } = string.Empty;
    public string TradeName { get; set; } = string.Empty;
    public CompanyCategory Category { get; set; }
    
    // Address fields
    public string ZipCode { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public string? Complement { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
