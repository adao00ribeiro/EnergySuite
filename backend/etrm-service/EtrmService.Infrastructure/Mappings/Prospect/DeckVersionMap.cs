using EtrmService.Domain.Entities.Prospect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings.Prospect;

public class DeckVersionMap : IEntityTypeConfiguration<DeckVersion>
{
    public void Configure(EntityTypeBuilder<DeckVersion> builder)
    {
        builder.ToTable("prospect_deck_versions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(v => v.DeckId).HasColumnName("deck_id").IsRequired();
        builder.Property(v => v.VersionNumber).HasColumnName("version_number").IsRequired();
        builder.Property(v => v.StoragePath).HasColumnName("storage_path").IsRequired().HasMaxLength(1000);
        builder.Property(v => v.ChangeReason).HasColumnName("change_reason").HasMaxLength(255);
        builder.Property(v => v.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
    }
}
