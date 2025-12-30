using Maliev.PerformanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.PerformanceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity framework configuration for the <see cref="PerformanceReview"/> entity.
/// </summary>
public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("performance_reviews");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmployeeId).IsRequired();
        builder.Property(x => x.ReviewerId).IsRequired();
        builder.Property(x => x.ReviewCycle).IsRequired();
        builder.Property(x => x.ReviewPeriodStart).IsRequired();
        builder.Property(x => x.ReviewPeriodEnd).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CreatedDate).IsRequired();

        builder.HasIndex(x => x.EmployeeId).HasDatabaseName("idx_perf_reviews_employee");
        builder.HasIndex(x => x.ReviewerId).HasDatabaseName("idx_perf_reviews_reviewer");
        builder.HasIndex(x => x.Status).HasDatabaseName("idx_perf_reviews_status");

        // Note: Unique index for overlap prevention is typically handled in application logic 
        // because SQL level overlapping checks are complex (start1 < end2 AND start2 < end1).
        // However, we can add a check constraint for start < end.
        builder.ToTable(t => t.HasCheckConstraint("CK_PerformanceReview_Period", "review_period_start < review_period_end"));
    }
}
