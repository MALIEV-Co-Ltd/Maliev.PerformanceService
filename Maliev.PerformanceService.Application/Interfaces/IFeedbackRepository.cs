using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Repository for managing 360-degree review feedback.
/// </summary>
public interface IFeedbackRepository
{
    /// <summary>
    /// Retrieves all feedback associated with a specific performance review.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of review feedback.</returns>
    Task<IEnumerable<ReviewFeedback>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new review feedback entry.
    /// </summary>
    /// <param name="feedback">The feedback entity to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created feedback entity.</returns>
    Task<ReviewFeedback> CreateAsync(ReviewFeedback feedback, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of feedback entries for a review, filtered by feedback type.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <param name="feedbackType">The type of feedback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The count of feedback entries.</returns>
    Task<int> GetFeedbackCountByTypeAsync(Guid reviewId, int feedbackType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total feedback entries for an employee across all their reviews.
    /// </summary>
    Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
}