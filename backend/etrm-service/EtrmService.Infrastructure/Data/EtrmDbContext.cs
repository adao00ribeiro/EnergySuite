using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;

namespace EtrmService.Infrastructure.Data;

public class EtrmDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public EtrmDbContext(DbContextOptions<EtrmDbContext> options, ICurrentUserService currentUserService) : base(options) 
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Contract> Contracts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EtrmDbContext).Assembly);
        
        // Multi-Tenant Global Query Filter
        modelBuilder.Entity<Contract>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        
        base.OnModelCreating(modelBuilder);
    }
}
