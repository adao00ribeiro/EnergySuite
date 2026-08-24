using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Data;

public class EtrmDbContext : DbContext
{
    public EtrmDbContext(DbContextOptions<EtrmDbContext> options) : base(options) { }

    public DbSet<Contract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CounterpartyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.Submarket).IsRequired();
            entity.Property(e => e.VolumeMwMed).HasPrecision(18, 4);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        base.OnModelCreating(modelBuilder);
    }
}
