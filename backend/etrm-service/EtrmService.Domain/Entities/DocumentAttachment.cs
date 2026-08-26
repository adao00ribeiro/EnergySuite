using System;

namespace EtrmService.Domain.Entities;

public class DocumentAttachment
{
    public Guid Id { get; private set; }
    
    // Type of entity it is attached to (e.g., "Contract", "Operation")
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public string BucketName { get; private set; } = string.Empty;
    public string ObjectKey { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }

    protected DocumentAttachment() { }

    public DocumentAttachment(string entityType, Guid entityId, string fileName, string contentType, string bucketName, string objectKey)
    {
        Id = Guid.NewGuid();
        EntityType = entityType;
        EntityId = entityId;
        FileName = fileName;
        ContentType = contentType;
        BucketName = bucketName;
        ObjectKey = objectKey;
        UploadedAt = DateTime.UtcNow;
    }
}
