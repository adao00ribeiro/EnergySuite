using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class PrecipitationScenarioMap : IEntityTypeConfiguration<PrecipitationScenario>
{
    public void Configure(EntityTypeBuilder<PrecipitationScenario> builder)
    {
        builder.ToTable("precipitation_scenarios");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.SourceType)
            .HasColumnName("source_type")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.ReferenceDate)
            .HasColumnName("reference_date")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.HorizonDays)
            .HasColumnName("horizon_days")
            .IsRequired();

        // Relationship
        builder.HasMany(x => x.Executions)
            .WithOne(e => e.Scenario)
            .HasForeignKey(e => e.ScenarioId)
            .HasConstraintName("fk_precipitation_scenario_executions")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
