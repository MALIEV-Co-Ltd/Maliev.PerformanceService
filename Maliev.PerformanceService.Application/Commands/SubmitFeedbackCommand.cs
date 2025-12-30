using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to submit performance feedback for a review.
/// </summary>
/// <param name="PerformanceReviewId">The identifier of the review.</param>
/// <param name="ProviderId">The identifier of the user providing feedback.</param>
/// <param name="FeedbackType">The type of feedback.</param>
/// <param name="Feedback">The feedback text.</param>
/// <param name="IsAnonymous">Whether the feedback is anonymous.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record SubmitFeedbackCommand(
    Guid PerformanceReviewId,
    Guid ProviderId,
    FeedbackType FeedbackType,
    string Feedback,
    bool IsAnonymous,
    Guid RequestingUserId);
