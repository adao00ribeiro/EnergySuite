using System;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities.Prospect;
using MediatR;

namespace EtrmService.Application.Prospect.Commands;

public class CreateStudyCommandHandler : IRequestHandler<CreateStudyCommand, Guid>
{
    private readonly IEtrmDbContext _context;

    public CreateStudyCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStudyCommand request, CancellationToken cancellationToken)
    {
        var study = new Study(
            request.Name,
            request.Description,
            request.Model,
            request.StartDate,
            request.HorizonMonths,
            request.UserId,
            request.TenantId
        );

        _context.ProspectStudies.Add(study);
        await _context.SaveChangesAsync(cancellationToken);

        return study.Id;
    }
}
