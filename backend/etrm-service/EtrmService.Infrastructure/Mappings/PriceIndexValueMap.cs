using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class PriceIndexValueMap : IEntityTypeConfiguration<PriceIndexValue>
{
    public void Configure(EntityTypeBuilder<PriceIndexValue> builder)
    {
        builder.ToTable("price_index_values");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.IndexType)
               .HasColumnName("index_type")
               .IsRequired();

        builder.Property(x => x.ReferenceMonth)
               .HasColumnName("reference_month")
               .HasMaxLength(7)
               .IsRequired();

        builder.Property(x => x.MonthlyRate)
               .HasColumnName("monthly_rate")
               .HasColumnType("decimal(18,6)")
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Unique index para impedir dois valores para o mesmo índice no mesmo mês
        builder.HasIndex(x => new { x.IndexType, x.ReferenceMonth })
               .IsUnique();
    }
}
