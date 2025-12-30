namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve aggregated feedback for a performance review.
/// </summary>
/// <param name="PerformanceReviewId">The identifier of the review.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record GetFeedbackQuery(Guid PerformanceReviewId, Guid RequestingUserId);
