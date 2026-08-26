using System;

namespace EtrmService.Application.Pluvia.DTOs;

public class ExportFileDto
{
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty; // e.g. "PREVS", "ENA", "VNA"
    public string DownloadUrl { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
