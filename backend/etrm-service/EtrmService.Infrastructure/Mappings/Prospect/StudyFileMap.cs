using EtrmService.Domain.Entities.Prospect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings.Prospect;

public class StudyFileMap : IEntityTypeConfiguration<StudyFile>
{
    public void Configure(EntityTypeBuilder<StudyFile> builder)
    {
        builder.ToTable("prospect_study_files");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(f => f.StudyId).HasColumnName("study_id").IsRequired();
        builder.Property(f => f.FileName).HasColumnName("file_name").IsRequired().HasMaxLength(255);
        builder.Property(f => f.StoragePath).HasColumnName("storage_path").IsRequired().HasMaxLength(1000);
        builder.Property(f => f.FileType).HasColumnName("file_type").IsRequired().HasMaxLength(20);
        builder.Property(f => f.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(f => f.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    }
}
