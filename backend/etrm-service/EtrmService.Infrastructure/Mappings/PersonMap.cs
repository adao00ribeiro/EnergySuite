using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class PersonMap : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("persons");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();

        builder.Property(p => p.Cpf).HasColumnName("cpf").HasMaxLength(11).IsRequired();
        builder.HasIndex(p => p.Cpf).IsUnique();

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.AdditionalCharacteristics).HasColumnName("additional_characteristics");

        // Address (Owned Type)
        builder.OwnsOne(p => p.Address, a =>
        {
            a.Property(x => x.ZipCode).HasColumnName("address_zip_code").HasMaxLength(10);
            a.Property(x => x.Street).HasColumnName("address_street").HasMaxLength(200);
            a.Property(x => x.Number).HasColumnName("address_number").HasMaxLength(20);
            a.Property(x => x.Complement).HasColumnName("address_complement").HasMaxLength(100);
            a.Property(x => x.Neighborhood).HasColumnName("address_neighborhood").HasMaxLength(100);
            a.Property(x => x.City).HasColumnName("address_city").HasMaxLength(100);
            a.Property(x => x.State).HasColumnName("address_state").HasMaxLength(2);
        });

        // Contact Info (Owned Type)
        builder.OwnsOne(p => p.ContactInfo, ci =>
        {
            ci.Property(x => x.GeneralEmail).HasColumnName("contact_general_email").HasMaxLength(100);
            ci.Property(x => x.LegalEmail).HasColumnName("contact_legal_email").HasMaxLength(100);
            ci.Property(x => x.FinancialEmail).HasColumnName("contact_financial_email").HasMaxLength(100);
            ci.Property(x => x.Phone1).HasColumnName("contact_phone1").HasMaxLength(20);
            ci.Property(x => x.Phone2).HasColumnName("contact_phone2").HasMaxLength(20);
            ci.Property(x => x.Phone3).HasColumnName("contact_phone3").HasMaxLength(20);
        });

        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
    }
}
