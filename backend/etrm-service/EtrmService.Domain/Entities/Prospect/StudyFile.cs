using System;

namespace EtrmService.Domain.Entities.Prospect;

public class StudyFile
{
    public Guid Id { get; private set; }
    public Guid StudyId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public string FileType { get; private set; } = string.Empty; // e.g., ZIP, DAT, XLSX
    public long SizeBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }

    public Study Study { get; private set; } = null!;

    protected StudyFile() { }

    public StudyFile(Guid studyId, string fileName, string storagePath, string fileType, long sizeBytes)
    {
        Id = Guid.NewGuid();
        StudyId = studyId;
        FileName = fileName;
        StoragePath = storagePath;
        FileType = fileType;
        SizeBytes = sizeBytes;
        UploadedAt = DateTime.UtcNow;
    }
}
