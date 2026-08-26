using System;
using MediatR;

namespace EtrmService.Application.Pluvia.Commands;

public class BlendCustomMapCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }
    public int HorizonDays { get; set; }
    
    // JSON representing the blending configuration, e.g. {"GEFS": 50, "ETA": 30, "ECMWF": 20}
    public string BlendConfig { get; set; } = string.Empty;
}
