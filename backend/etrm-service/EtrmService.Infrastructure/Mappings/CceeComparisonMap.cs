using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class CceeComparisonMap : IEntityTypeConfiguration<CceeComparison>
{
    public void Configure(EntityTypeBuilder<CceeComparison> builder)
    {
        builder.ToTable("ccee_comparisons");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(c => c.OperationId)
            .HasColumnName("operation_id")
            .IsRequired(false);

        builder.Property(c => c.CounterpartyId)
            .HasColumnName("counterparty_id")
            .IsRequired(false);

        builder.Property(c => c.CounterpartyCceeCode)
            .HasColumnName("counterparty_ccee_code")
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(c => c.Period)
            .HasColumnName("period")
            .IsRequired();

        builder.Property(c => c.BackOpsVolume)
            .HasColumnName("backops_volume")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(c => c.CceeVolume)
            .HasColumnName("ccee_volume")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(c => c.Difference)
            .HasColumnName("difference")
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);
    }
}
