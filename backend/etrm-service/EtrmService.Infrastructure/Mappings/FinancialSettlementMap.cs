using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class FinancialSettlementMap : IEntityTypeConfiguration<FinancialSettlement>
{
    public void Configure(EntityTypeBuilder<FinancialSettlement> builder)
    {
        builder.ToTable("financial_settlements");
        builder.HasKey(fs => fs.Id);

        builder.Property(fs => fs.Id).HasColumnName("id");
        builder.Property(fs => fs.BillingId).HasColumnName("billing_id").IsRequired(false);
        builder.Property(fs => fs.CounterpartyId).HasColumnName("counterparty_id").IsRequired();
        builder.Property(fs => fs.TenantId).HasColumnName("tenant_id").IsRequired();
        
        builder.Property(fs => fs.Type).HasColumnName("type").HasConversion<string>().IsRequired();
        builder.Property(fs => fs.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(fs => fs.DueDate).HasColumnName("due_date").IsRequired();
        builder.Property(fs => fs.ReferenceMonth).HasColumnName("reference_month").HasMaxLength(7).IsRequired();
        
        builder.Property(fs => fs.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(fs => fs.OffsetGroupId).HasColumnName("offset_group_id").IsRequired(false);
        builder.Property(fs => fs.CreatedAt).HasColumnName("created_at").IsRequired();

        builder.HasOne(fs => fs.Billing)
               .WithMany()
               .HasForeignKey(fs => fs.BillingId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(fs => fs.Counterparty)
               .WithMany()
               .HasForeignKey(fs => fs.CounterpartyId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.HasIndex(fs => new { fs.CounterpartyId, fs.ReferenceMonth, fs.Status });
    }
}
