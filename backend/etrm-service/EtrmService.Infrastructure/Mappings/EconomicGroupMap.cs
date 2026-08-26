using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class EconomicGroupMap : IEntityTypeConfiguration<EconomicGroup>
{
    public void Configure(EntityTypeBuilder<EconomicGroup> builder)
    {
        builder.ToTable("economic_groups");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired();

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();

        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
    }
}
