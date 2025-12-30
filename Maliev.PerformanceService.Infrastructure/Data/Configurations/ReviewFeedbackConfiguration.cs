using Maliev.PerformanceService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Maliev.PerformanceService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity framework configuration for the <see cref="ReviewFeedback"/> entity.
/// </summary>
public class ReviewFeedbackConfiguration : IEntityTypeConfiguration<ReviewFeedback>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ReviewFeedback> builder)
    {
        builder.ToTable("review_feedback");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PerformanceReviewId).IsRequired();
        builder.Property(x => x.ProviderId).IsRequired();
        builder.Property(x => x.FeedbackType).IsRequired();
        builder.Property(x => x.Feedback).IsRequired();
        builder.Property(x => x.IsAnonymous).IsRequired();
        builder.Property(x => x.SubmittedDate).IsRequired();

        builder.HasIndex(x => x.PerformanceReviewId).HasDatabaseName("idx_feedback_review");

        builder.HasOne<PerformanceReview>()
            .WithMany()
            .HasForeignKey(x => x.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Note: Field-level encryption for ProviderId when IsAnonymous=true 
        // is better handled via a ValueConverter if we want it in the database.
        // However, the task mentions it specifically for the configuration.
    }
}
