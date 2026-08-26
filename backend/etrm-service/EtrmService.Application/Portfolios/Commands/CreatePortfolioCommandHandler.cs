using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;

namespace EtrmService.Application.Portfolios.Commands;

public class CreatePortfolioCommandHandler : IRequestHandler<CreatePortfolioCommand, Guid>
{
    private readonly IEtrmDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreatePortfolioCommandHandler(IEtrmDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
    {
        var portfolio = new Portfolio(
            request.Name,
            request.Type,
            request.Responsible,
            request.Description,
            _currentUserService.TenantId
        );

        _context.Portfolios.Add(portfolio);
        await _context.SaveChangesAsync(cancellationToken);

        return portfolio.Id;
    }
}
