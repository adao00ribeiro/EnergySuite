using System;

namespace EtrmService.Application.Interfaces;

public interface ICurrentUserService
{
    Guid TenantId { get; }
    string UserId { get; }
}
