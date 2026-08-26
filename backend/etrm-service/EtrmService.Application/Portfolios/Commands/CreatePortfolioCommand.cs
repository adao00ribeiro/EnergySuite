using System;
using MediatR;

namespace EtrmService.Application.Portfolios.Commands;

public class CreatePortfolioCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Responsible { get; set; } = string.Empty;
}
