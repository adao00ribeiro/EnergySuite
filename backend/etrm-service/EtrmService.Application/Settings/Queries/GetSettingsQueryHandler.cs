using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtrmService.Application.Interfaces;
using EtrmService.Application.Settings.DTOs;
using EtrmService.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Settings.Queries;

public class GetSettingsQueryHandler : IRequestHandler<GetSettingsQuery, SettingsDto>
{
    private readonly IEtrmDbContext _context;

    public GetSettingsQueryHandler(IEtrmDbContext context)
    {
        _context = context;
    }

    public async Task<SettingsDto> Handle(GetSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _context.AppSettings
            .AsNoTracking()
            .Where(s => s.TenantId == request.TenantId)
            .ToListAsync(cancellationToken);

        return new SettingsDto
        {
            Theme = GetValue(settings, "theme"),
            Language = GetValue(settings, "language"),
            Timezone = GetValue(settings, "timezone")
        };
    }

    private static string GetValue(IReadOnlyCollection<AppSetting> settings, string key)
        => settings.FirstOrDefault(s => s.Key == key)?.Value ?? string.Empty;
}
