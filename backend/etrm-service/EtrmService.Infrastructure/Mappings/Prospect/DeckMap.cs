using EtrmService.Domain.Entities.Prospect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings.Prospect;

public class DeckMap : IEntityTypeConfiguration<Deck>
{
    public void Configure(EntityTypeBuilder<Deck> builder)
    {
        builder.ToTable("prospect_decks");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.StudyId).HasColumnName("study_id").IsRequired();
        builder.Property(d => d.Model).HasColumnName("model").IsRequired().HasMaxLength(50);
        builder.Property(d => d.Period).HasColumnName("period").IsRequired();
        builder.Property(d => d.SequenceOrder).HasColumnName("sequence_order").IsRequired();
        builder.Property(d => d.State).HasColumnName("state").IsRequired();
        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();

        builder.HasMany(d => d.Versions)
            .WithOne(v => v.Deck)
            .HasForeignKey(v => v.DeckId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
