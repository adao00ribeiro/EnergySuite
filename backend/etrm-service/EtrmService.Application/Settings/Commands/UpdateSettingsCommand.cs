using System;
using MediatR;
using EtrmService.Application.Settings.DTOs;

namespace EtrmService.Application.Settings.Commands;

public record UpdateSettingsCommand(Guid TenantId, SettingsDto Settings) : IRequest<bool>;
