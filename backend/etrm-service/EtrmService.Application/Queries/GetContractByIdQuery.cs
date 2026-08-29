using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Queries.DTOs;

namespace EtrmService.Application.Queries;

public record GetContractByIdQuery(Guid Id) : IRequest<ContractDto?>;

public class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
{
    private readonly IEtrmDbContext _context;

    public GetContractByIdQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
    {
        var contract = await _context.Contracts
            .AsNoTracking()
            .Include(c => c.Amendments)
            .SingleOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (contract == null)
            return null;

        return new ContractDto
        {
            Id = contract.Id,
            CounterpartyName = contract.CounterpartyName,
            Type = contract.Type.ToString(),
            Submarket = contract.Submarket.ToString(),
            VolumeMwMed = contract.VolumeMwMed,
            Price = contract.Price,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt,
            Version = contract.Version,
            PriceIndexType = contract.PriceIndexType.ToString(),
            FlexibilityMargin = contract.FlexibilityMargin,
            Amendments = contract.Amendments
                .OrderBy(a => a.Version)
                .Select(a => new ContractAmendmentDto
                {
                    Id = a.Id,
                    Version = a.Version,
                    Description = a.Description,
                    EffectiveDate = a.EffectiveDate,
                    PreviousPrice = a.PreviousPrice,
                    NewPrice = a.NewPrice,
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        };
    }
}
