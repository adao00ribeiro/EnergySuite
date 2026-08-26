using EtrmService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EtrmService.Infrastructure.Mappings;

public class ModelExecutionMap : IEntityTypeConfiguration<ModelExecution>
{
    public void Configure(EntityTypeBuilder<ModelExecution> builder)
    {
        builder.ToTable("model_executions");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .HasColumnName("id");

        builder.Property(x => x.ScenarioId)
            .HasColumnName("scenario_id")
            .IsRequired();

        builder.Property(x => x.ModelType)
            .HasColumnName("model_type")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at");

        // Relationship
        builder.HasMany(x => x.Results)
            .WithOne(r => r.Execution)
            .HasForeignKey(r => r.ExecutionId)
            .HasConstraintName("fk_model_execution_results")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
