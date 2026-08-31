using System;
using MediatR;
using EtrmService.Application.Settings.DTOs;

namespace EtrmService.Application.Settings.Queries;

public record GetSettingsQuery(Guid TenantId) : IRequest<SettingsDto>;
