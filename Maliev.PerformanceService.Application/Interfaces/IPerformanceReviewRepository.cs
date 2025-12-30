using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Repository for managing performance reviews.
/// </summary>
public interface IPerformanceReviewRepository
{
    /// <summary>
    /// Gets a performance review by its unique identifier.
    /// </summary>
    /// <param name="id">The review identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The review entity or null if not found.</returns>
    Task<PerformanceReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all performance reviews for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of performance reviews.</returns>
    Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new performance review.
    /// </summary>
    /// <param name="review">The review entity to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created review entity.</returns>
    Task<PerformanceReview> CreateAsync(PerformanceReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing performance review.
    /// </summary>
    /// <param name="review">The review entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated review entity.</returns>
    Task<PerformanceReview> UpdateAsync(PerformanceReview review, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a performance review by its identifier.
    /// </summary>
    /// <param name="id">The review identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an active (non-completed) overlapping review period already exists for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="reviewCycle">The review cycle type.</param>
    /// <param name="periodStart">The start date of the period.</param>
    /// <param name="periodEnd">The end date of the period.</param>
    /// <param name="excludeReviewId">Optional identifier to exclude from the check (e.g., when updating).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an overlapping review exists, otherwise false.</returns>
    Task<bool> ExistsOverlappingReviewAsync(Guid employeeId, int reviewCycle, DateTime periodStart, DateTime periodEnd, Guid? excludeReviewId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves reviews that are pending self-assessment or manager review.
    /// </summary>
    Task<IEnumerable<PerformanceReview>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total reviews for an employee.
    /// </summary>
    Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves reviews older than a certain date for archival.
    /// </summary>
    Task<IEnumerable<PerformanceReview>> GetReviewsForArchivalAsync(DateTime olderThan, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a review as archived.
    /// </summary>
    Task MarkAsArchivedAsync(Guid id, CancellationToken cancellationToken = default);
}