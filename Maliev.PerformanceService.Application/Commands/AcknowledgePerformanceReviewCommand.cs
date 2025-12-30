namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to acknowledge a performance review.
/// </summary>
/// <param name="ReviewId">The unique identifier of the review.</param>
/// <param name="AcknowledgedBy">The unique identifier of the employee acknowledging the review.</param>
public record AcknowledgePerformanceReviewCommand(Guid ReviewId, Guid AcknowledgedBy);
