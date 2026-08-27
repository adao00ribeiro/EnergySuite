using System.Threading;
using System.Threading.Tasks;
using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EtrmService.Application.Interfaces;

public interface IEtrmDbContext
{
    DbSet<Contract> Contracts { get; }
    DbSet<Company> Companies { get; }
    DbSet<Person> Persons { get; }
    DbSet<EconomicGroup> EconomicGroups { get; }
    DbSet<Ticket> Tickets { get; }
    DbSet<Operation> Operations { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Portfolio> Portfolios { get; }
    DbSet<PrecipitationScenario> PrecipitationScenarios { get; }
    DbSet<ModelExecution> ModelExecutions { get; }
    DbSet<HydrologicalResult> HydrologicalResults { get; set; }
    DbSet<ForecastMetadata> ForecastMetadatas { get; set; }
    DbSet<CustomScenario> CustomScenarios { get; set; }
    
    // Sprint 3
    DbSet<ContractAmendment> ContractAmendments { get; set; }
    DbSet<PriceIndexValue> PriceIndexValues { get; set; }
    DbSet<DocumentAttachment> DocumentAttachments { get; set; }
    
    // Sprint 4
    DbSet<Billing> Billings { get; set; }
    DbSet<FinancialSettlement> FinancialSettlements { get; set; }

    // Sprint 5
    DbSet<CceeComparison> CceeComparisons { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
