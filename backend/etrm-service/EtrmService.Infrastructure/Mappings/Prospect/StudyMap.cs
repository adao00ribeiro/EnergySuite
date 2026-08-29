using EtrmService.Domain.Entities.Prospect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings.Prospect;

public class StudyMap : IEntityTypeConfiguration<Study>
{
    public void Configure(EntityTypeBuilder<Study> builder)
    {
        builder.ToTable("prospect_studies");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(s => s.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(s => s.Model).HasColumnName("model").IsRequired().HasMaxLength(50);
        builder.Property(s => s.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(s => s.HorizonMonths).HasColumnName("horizon_months").IsRequired();
        builder.Property(s => s.State).HasColumnName("state").IsRequired();
        builder.Property(s => s.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.ResultsJson).HasColumnName("results_json").HasColumnType("text").IsRequired(false);

        builder.HasMany(s => s.Tags)
            .WithOne(t => t.Study)
            .HasForeignKey(t => t.StudyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Files)
            .WithOne(f => f.Study)
            .HasForeignKey(f => f.StudyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
