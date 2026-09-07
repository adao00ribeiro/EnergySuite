using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class SimulationMap : IEntityTypeConfiguration<Simulation>
{
    public void Configure(EntityTypeBuilder<Simulation> builder)
    {
        builder.ToTable("Simulations");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Notes)
            .HasMaxLength(1000);

        builder.Property(s => s.NetPositionBeforeMwMed)
            .HasPrecision(18, 4);

        builder.Property(s => s.NetPositionAfterMwMed)
            .HasPrecision(18, 4);

        builder.HasOne(s => s.Opportunity)
            .WithMany()
            .HasForeignKey(s => s.OpportunityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.TenantId, s.SimulatedAt });
    }
}
