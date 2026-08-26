using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using EtrmService.Domain.ValueObjects;

namespace EtrmService.Application.CommercialRegistry.Commands;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateCompanyCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company(
            request.Cnpj,
            request.CorporateName,
            request.TradeName,
            request.Category,
            _currentUserService.TenantId
        );

        var address = new Address(
            request.ZipCode,
            request.Street,
            request.Number,
            request.Complement,
            request.Neighborhood,
            request.City,
            request.State
        );
        company.UpdateAddress(address);
        
        // Init empty contact info for now
        company.UpdateContactInfo(new ContactInfo(null, null, null, null, null, null));

        _context.Companies.Add(company);
        await _context.SaveChangesAsync(cancellationToken);

        return company.Id;
    }
}
