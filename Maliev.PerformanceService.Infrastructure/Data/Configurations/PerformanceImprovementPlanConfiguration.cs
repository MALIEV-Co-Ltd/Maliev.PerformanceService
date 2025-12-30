using Maliev.PerformanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.PerformanceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity framework configuration for the <see cref="PerformanceImprovementPlan"/> entity.
/// </summary>
public class PerformanceImprovementPlanConfiguration : IEntityTypeConfiguration<PerformanceImprovementPlan>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PerformanceImprovementPlan> builder)
    {
        builder.ToTable("performance_improvement_plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.InitiatorId).IsRequired();
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Reason).IsRequired();
        builder.Property(x => x.ImprovementAreas).IsRequired();
        builder.Property(x => x.SuccessCriteria).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedDate).IsRequired();

        builder.HasIndex(x => x.EmployeeId).HasDatabaseName("idx_pips_employee");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_pips_status");

        builder.ToTable(t => t.HasCheckConstraint("CK_PIP_DateRange", "start_date < end_date"));
    }
}
