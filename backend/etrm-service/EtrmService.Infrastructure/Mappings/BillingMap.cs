using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class BillingMap : IEntityTypeConfiguration<Billing>
{
    public void Configure(EntityTypeBuilder<Billing> builder)
    {
        builder.ToTable("billings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.OperationId).HasColumnName("operation_id").IsRequired();
        builder.Property(b => b.ReferenceMonth).HasColumnName("reference_month").HasMaxLength(7).IsRequired(); // e.g., "2026-08"
        
        builder.Property(b => b.CalculatedVolume).HasColumnName("calculated_volume").HasPrecision(18, 6).IsRequired();
        builder.Property(b => b.AppliedPrice).HasColumnName("applied_price").HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.GrossAmount).HasColumnName("gross_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.TaxesAmount).HasColumnName("taxes_amount").HasPrecision(18, 2).IsRequired();
        builder.Property(b => b.NetAmount).HasColumnName("net_amount").HasPrecision(18, 2).IsRequired();
        
        builder.Property(b => b.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(b => b.Operation)
               .WithMany()
               .HasForeignKey(b => b.OperationId)
               .OnDelete(DeleteBehavior.Cascade);
               
        builder.HasIndex(b => new { b.OperationId, b.ReferenceMonth }).IsUnique();
    }
}
