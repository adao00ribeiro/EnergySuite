using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class OpportunityMap : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunities");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.EstimatedVolumeMwMed)
            .HasPrecision(18, 4);

        builder.Property(o => o.EstimatedSpread)
            .HasPrecision(18, 4);

        builder.Property(o => o.OpportunityScore)
            .HasPrecision(18, 4);

        builder.Property(o => o.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(o => o.Portfolio)
            .WithMany()
            .HasForeignKey(o => o.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Strategy)
            .WithMany()
            .HasForeignKey(o => o.StrategyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(o => new { o.TenantId, o.Status });
    }
}
