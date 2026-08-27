namespace EtrmService.Domain.Enums;

public enum StudyState
{
    Created = 1,
    Uploading = 2,
    Ready = 3,
    Generating = 4,
    Generated = 5,
    Queued = 6,
    Running = 7,
    Processing = 8,
    Completed = 9,
    Failed = 10,
    Cancelled = 11
}
