using EtrmService.Domain.Entities.Prospect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings.Prospect;

public class StudyTagMap : IEntityTypeConfiguration<StudyTag>
{
    public void Configure(EntityTypeBuilder<StudyTag> builder)
    {
        builder.ToTable("prospect_study_tags");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(t => t.StudyId).HasColumnName("study_id").IsRequired();
        builder.Property(t => t.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
        builder.Property(t => t.ColorHex).HasColumnName("color_hex").HasMaxLength(7);
    }
}
