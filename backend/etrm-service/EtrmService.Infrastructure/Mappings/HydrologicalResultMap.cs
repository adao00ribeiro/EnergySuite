using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class HydrologicalResultMap : IEntityTypeConfiguration<HydrologicalResult>
{
    public void Configure(EntityTypeBuilder<HydrologicalResult> builder)
    {
        builder.ToTable("hydrological_results");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.ExecutionId)
            .HasColumnName("execution_id")
            .IsRequired();

        builder.Property(x => x.Submarket)
            .HasColumnName("submarket")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Basin)
            .HasColumnName("basin")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ValueMwMed)
            .HasColumnName("value_mw_med")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.ValuePercentageMlt)
            .HasColumnName("value_percentage_mlt")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.TargetDate)
            .HasColumnName("target_date")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
