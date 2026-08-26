using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class AuditLogMap : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.EntityName).HasColumnName("entity_name").IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).HasColumnName("entity_id").IsRequired().HasMaxLength(50);
        builder.Property(a => a.Action).HasColumnName("action").IsRequired().HasMaxLength(50);
        builder.Property(a => a.ChangesJson).HasColumnName("changes_json").HasColumnType("jsonb").IsRequired();
        builder.Property(a => a.ChangedBy).HasColumnName("changed_by").IsRequired().HasMaxLength(100);
        builder.Property(a => a.ChangedAt).HasColumnName("changed_at").IsRequired();
        builder.Property(a => a.TenantId).HasColumnName("tenant_id").IsRequired();
    }
}
