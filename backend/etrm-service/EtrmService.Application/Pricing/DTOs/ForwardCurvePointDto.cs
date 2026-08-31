namespace EtrmService.Application.Pricing.DTOs;

public class ForwardCurvePointDto
{
    public string Month { get; set; } = null!;
    public decimal PldSE { get; set; }
    public decimal PldS { get; set; }
    public decimal PldNE { get; set; }
    public decimal PldN { get; set; }
}
