using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the retrieval of a specific performance review by its identifier.
/// </summary>
public class GetPerformanceReviewByIdQueryHandler
{
    private readonly IPerformanceReviewRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPerformanceReviewByIdQueryHandler"/> class.
    /// </summary>
    public GetPerformanceReviewByIdQueryHandler(IPerformanceReviewRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a performance review.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The performance review or null if not found.</returns>
    public async Task<PerformanceReview?> HandleAsync(GetPerformanceReviewByIdQuery query, CancellationToken cancellationToken = default)
    {
        var review = await _repository.GetByIdAsync(query.ReviewId, cancellationToken);
        
        // TODO: Authorization check
        
        return review;
    }
}