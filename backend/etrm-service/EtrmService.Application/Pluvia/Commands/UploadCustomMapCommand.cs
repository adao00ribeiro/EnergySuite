using System;
using System.IO;
using MediatR;

namespace EtrmService.Application.Pluvia.Commands;

public class UploadCustomMapCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public DateTime ReferenceDate { get; set; }
    public int HorizonDays { get; set; }
    public string FileName { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = string.Empty;
}
