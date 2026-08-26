using System;

namespace EtrmService.Application.Pluvia.Queries;

public class EnaResultDto
{
    public DateTime TargetDate { get; set; }
    public decimal ValueMwMed { get; set; }
    public decimal ValuePercentageMlt { get; set; }
}
