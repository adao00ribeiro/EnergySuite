using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class OperationMap : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("operations");
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.TicketId).HasColumnName("ticket_id").IsRequired();
        builder.Property(o => o.PortfolioId).HasColumnName("portfolio_id").IsRequired();
        builder.Property(o => o.CounterpartyId).HasColumnName("counterparty_id").IsRequired();
        
        builder.Property(o => o.Type).HasColumnName("type").HasConversion<string>().IsRequired();
        builder.Property(o => o.State).HasColumnName("state").HasConversion<string>().IsRequired();
        
        builder.Property(o => o.VolumeMwMed).HasColumnName("volume_mwmed").HasPrecision(18, 6).IsRequired();
        builder.Property(o => o.Price).HasColumnName("price").HasPrecision(18, 2).IsRequired();
        builder.Property(o => o.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(o => o.EndDate).HasColumnName("end_date").IsRequired();
        builder.Property(o => o.TenantId).HasColumnName("tenant_id").IsRequired();

        builder.HasOne(o => o.Portfolio)
               .WithMany()
               .HasForeignKey(o => o.PortfolioId)
               .OnDelete(DeleteBehavior.Restrict);
               
        builder.HasOne(o => o.Counterparty)
               .WithMany()
               .HasForeignKey(o => o.CounterpartyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
