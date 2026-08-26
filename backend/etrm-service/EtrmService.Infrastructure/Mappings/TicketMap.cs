using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class TicketMap : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.ReferenceNumber).HasColumnName("reference_number").IsRequired().HasMaxLength(50);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(t => t.TenantId).HasColumnName("tenant_id").IsRequired();

        builder.HasMany(t => t.Operations)
               .WithOne(o => o.Ticket)
               .HasForeignKey(o => o.TicketId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
