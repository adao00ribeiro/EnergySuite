using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Infrastructure.Data;

public class EtrmDbContext : DbContext
{
    public EtrmDbContext(DbContextOptions<EtrmDbContext> options) : base(options) { }

    public DbSet<Contract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EtrmDbContext).Assembly);
        
        base.OnModelCreating(modelBuilder);
    }
}
