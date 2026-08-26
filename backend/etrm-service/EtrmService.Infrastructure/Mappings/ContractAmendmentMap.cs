using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class ContractAmendmentMap : IEntityTypeConfiguration<ContractAmendment>
{
    public void Configure(EntityTypeBuilder<ContractAmendment> builder)
    {
        builder.ToTable("contract_amendments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.ContractId)
               .HasColumnName("contract_id")
               .IsRequired();

        builder.Property(x => x.Version)
               .HasColumnName("version")
               .IsRequired();

        builder.Property(x => x.Description)
               .HasColumnName("description")
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.EffectiveDate)
               .HasColumnName("effective_date")
               .IsRequired();

        builder.Property(x => x.PreviousPrice)
               .HasColumnName("previous_price")
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.NewPrice)
               .HasColumnName("new_price")
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.PreviousVolumeMwMed)
               .HasColumnName("previous_volume_mw_med")
               .HasColumnType("decimal(18,4)")
               .IsRequired();

        builder.Property(x => x.NewVolumeMwMed)
               .HasColumnName("new_volume_mw_med")
               .HasColumnType("decimal(18,4)")
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
