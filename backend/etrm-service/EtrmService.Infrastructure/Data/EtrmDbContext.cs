using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using EtrmService.Application.Interfaces;

namespace EtrmService.Infrastructure.Data;

public class EtrmDbContext : DbContext, IEtrmDbContext
{
    private readonly ICurrentUserService _currentUserService;

    public EtrmDbContext(DbContextOptions<EtrmDbContext> options, ICurrentUserService currentUserService) : base(options) 
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Contract> Contracts { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Person> Persons { get; set; }
    public DbSet<EconomicGroup> EconomicGroups { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Portfolio> Portfolios { get; set; }
    public DbSet<PrecipitationScenario> PrecipitationScenarios { get; set; }
    public DbSet<ModelExecution> ModelExecutions { get; set; }
    public DbSet<HydrologicalResult> HydrologicalResults { get; set; }
    public DbSet<ForecastMetadata> ForecastMetadatas { get; set; }
    public DbSet<CustomScenario> CustomScenarios { get; set; }
    public DbSet<ContractAmendment> ContractAmendments { get; set; }
    public DbSet<PriceIndexValue> PriceIndexValues { get; set; }
    public DbSet<DocumentAttachment> DocumentAttachments { get; set; }
    
    // Sprint 4: Finance
    public DbSet<Billing> Billings { get; set; }
    public DbSet<FinancialSettlement> FinancialSettlements { get; set; }
    
    // Sprint 5: CCEE Integration
    public DbSet<CceeComparison> CceeComparisons { get; set; }

    // Sprint 6: B2B Integration & Webhooks
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; }

    // Prospect Module
    public DbSet<EtrmService.Domain.Entities.Prospect.Study> ProspectStudies { get; set; }
    public DbSet<EtrmService.Domain.Entities.Prospect.StudyTag> ProspectStudyTags { get; set; }
    public DbSet<EtrmService.Domain.Entities.Prospect.StudyFile> ProspectStudyFiles { get; set; }
    public DbSet<EtrmService.Domain.Entities.Prospect.Deck> ProspectDecks { get; set; }
    public DbSet<EtrmService.Domain.Entities.Prospect.DeckVersion> ProspectDeckVersions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EtrmDbContext).Assembly);
        
        // Multi-Tenant Global Query Filter
        modelBuilder.Entity<Contract>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Company>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Person>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<EconomicGroup>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Ticket>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Operation>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<AuditLog>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        modelBuilder.Entity<Portfolio>().HasQueryFilter(c => c.TenantId == _currentUserService.TenantId);
        
        base.OnModelCreating(modelBuilder);
    }
}
