using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class CompanyMap : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id").IsRequired();
        
        builder.Property(c => c.Cnpj).HasColumnName("cnpj").HasMaxLength(14).IsRequired();
        builder.HasIndex(c => c.Cnpj).IsUnique();

        builder.Property(c => c.CorporateName).HasColumnName("corporate_name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.TradeName).HasColumnName("trade_name").HasMaxLength(200).IsRequired();
        builder.Property(c => c.StateRegistration).HasColumnName("state_registration").HasMaxLength(50);
        builder.Property(c => c.EconomicActivity).HasColumnName("economic_activity").HasMaxLength(100);
        
        builder.Property(c => c.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // CCEE Data
        builder.Property(c => c.CceeProfile).HasColumnName("ccee_profile").HasMaxLength(100);
        builder.Property(c => c.CceeCode).HasColumnName("ccee_code").HasMaxLength(50);
        builder.Property(c => c.CceeAcronym).HasColumnName("ccee_acronym").HasMaxLength(50);
        builder.Property(c => c.Class)
            .HasColumnName("ccee_class")
            .HasConversion<string>()
            .HasMaxLength(50);

        // Address (Owned Type)
        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(p => p.ZipCode).HasColumnName("address_zip_code").HasMaxLength(10);
            a.Property(p => p.Street).HasColumnName("address_street").HasMaxLength(200);
            a.Property(p => p.Number).HasColumnName("address_number").HasMaxLength(20);
            a.Property(p => p.Complement).HasColumnName("address_complement").HasMaxLength(100);
            a.Property(p => p.Neighborhood).HasColumnName("address_neighborhood").HasMaxLength(100);
            a.Property(p => p.City).HasColumnName("address_city").HasMaxLength(100);
            a.Property(p => p.State).HasColumnName("address_state").HasMaxLength(2);
        });

        // Contact Info (Owned Type)
        builder.OwnsOne(c => c.ContactInfo, ci =>
        {
            ci.Property(p => p.GeneralEmail).HasColumnName("contact_general_email").HasMaxLength(100);
            ci.Property(p => p.LegalEmail).HasColumnName("contact_legal_email").HasMaxLength(100);
            ci.Property(p => p.FinancialEmail).HasColumnName("contact_financial_email").HasMaxLength(100);
            ci.Property(p => p.Phone1).HasColumnName("contact_phone1").HasMaxLength(20);
            ci.Property(p => p.Phone2).HasColumnName("contact_phone2").HasMaxLength(20);
            ci.Property(p => p.Phone3).HasColumnName("contact_phone3").HasMaxLength(20);
        });

        // Relationships
        builder.Property(c => c.EconomicGroupId).HasColumnName("economic_group_id");
        builder.HasOne(c => c.EconomicGroup)
            .WithMany(eg => eg.Companies)
            .HasForeignKey(c => c.EconomicGroupId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("fk_company_economic_group");

        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
    }
}
