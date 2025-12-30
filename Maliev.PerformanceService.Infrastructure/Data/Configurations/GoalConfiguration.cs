using Maliev.PerformanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.PerformanceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity framework configuration for the <see cref="Goal"/> entity.
/// </summary>
public class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.Description).IsRequired();
        builder.Property(x => x.TargetCompletionDate).IsRequired();
        builder.Property(x => x.CurrentStatus).IsRequired();
        builder.Property(x => x.CreatedDate).IsRequired();

        builder.HasIndex(x => x.EmployeeId).HasDatabaseName("idx_goals_employee");
        builder.HasIndex(x => x.PerformanceReviewId).HasDatabaseName("idx_goals_review");
        builder.HasIndex(x => x.CurrentStatus).HasDatabaseName("idx_goals_status");

        builder.HasOne<PerformanceReview>()
            .WithMany()
            .HasForeignKey(x => x.PerformanceReviewId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.ToTable(t => t.HasCheckConstraint("CK_Goal_TargetDate", "target_completion_date > created_date"));
    }
}
