using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Settings.Commands;

public class UpdateSettingsCommandHandler : IRequestHandler<UpdateSettingsCommand, bool>
{
    private readonly IEtrmDbContext _context;

    public UpdateSettingsCommandHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateSettingsCommand request, CancellationToken cancellationToken)
    {
        if (request.Settings == null) return false;

        await UpsertAsync(request.TenantId, "theme", request.Settings.Theme, cancellationToken);
        await UpsertAsync(request.TenantId, "language", request.Settings.Language, cancellationToken);
        await UpsertAsync(request.TenantId, "timezone", request.Settings.Timezone, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task UpsertAsync(System.Guid tenantId, string key, string? value, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var existing = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.Key == key, cancellationToken);

        if (existing == null)
            _context.AppSettings.Add(new AppSetting(tenantId, key, value));
        else
            existing.UpdateValue(value);
    }
}
