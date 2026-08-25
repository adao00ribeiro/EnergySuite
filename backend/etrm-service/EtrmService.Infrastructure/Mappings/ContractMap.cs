using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EtrmService.Domain.Entities;

namespace EtrmService.Infrastructure.Mappings;

public class ContractMap : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.CounterpartyName)
               .HasColumnName("counterparty_name")
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Type)
               .HasColumnName("type")
               .IsRequired();

        builder.Property(x => x.Submarket)
               .HasColumnName("submarket")
               .IsRequired();

        builder.Property(x => x.VolumeMwMed)
               .HasColumnName("volume_mw_med")
               .HasColumnType("decimal(18,4)")
               .IsRequired();

        builder.Property(x => x.Price)
               .HasColumnName("price")
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(x => x.StartDate)
               .HasColumnName("start_date")
               .IsRequired();

        builder.Property(x => x.EndDate)
               .HasColumnName("end_date")
               .IsRequired();

        builder.Property(x => x.CreatedAt)
               .HasColumnName("created_at")
               .IsRequired()
               .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
