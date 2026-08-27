using System.Collections.Generic;

namespace EtrmService.Application.Prospect.DTOs;

public class StudyResultDto
{
    public string Month { get; set; }
    public decimal PldSE { get; set; }
    public decimal PldS { get; set; }
    public decimal PldNE { get; set; }
    public decimal PldN { get; set; }
}

public class StudyResultResponseDto
{
    public List<StudyResultDto> Results { get; set; } = new();
}
