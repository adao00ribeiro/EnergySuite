using System;

namespace EtrmService.Domain.Entities;

public class ForecastMetadata
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // e.g. "GEFS", "ETA", "ECMWF"
    public string ModelName { get; set; } = string.Empty;
    
    // The date the forecast was generated for
    public DateTime ReferenceDate { get; set; }
    
    // E.g. "0p50", "15km"
    public string Resolution { get; set; } = string.Empty;
    
    // Number of available ensemble members, if applicable
    public int EnsembleMembers { get; set; }
    
    // S3/MinIO Path where the raw data is stored
    public string LakehousePath { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
