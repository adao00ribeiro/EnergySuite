using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class DocumentAttachmentMap : IEntityTypeConfiguration<DocumentAttachment>
{
    public void Configure(EntityTypeBuilder<DocumentAttachment> builder)
    {
        builder.ToTable("document_attachments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(d => d.EntityType).HasColumnName("entity_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(d => d.FileName).HasColumnName("file_name").HasMaxLength(500).IsRequired();
        builder.Property(d => d.ContentType).HasColumnName("content_type").HasMaxLength(100).IsRequired();
        builder.Property(d => d.BucketName).HasColumnName("bucket_name").HasMaxLength(100).IsRequired();
        builder.Property(d => d.ObjectKey).HasColumnName("object_key").HasMaxLength(1000).IsRequired();
        builder.Property(d => d.UploadedAt).HasColumnName("uploaded_at").IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        // Index for faster lookups when querying attachments for a specific entity
        builder.HasIndex(d => new { d.EntityType, d.EntityId });
    }
}
