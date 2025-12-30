namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve a specific performance review by its identifier.
/// </summary>
/// <param name="ReviewId">The identifier of the review.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record GetPerformanceReviewByIdQuery(
    Guid ReviewId,
    Guid RequestingUserId);