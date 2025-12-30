using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Data transfer object for performance feedback.
/// </summary>
/// <param name="Id">The unique identifier of the feedback.</param>
/// <param name="PerformanceReviewId">The identifier of the linked performance review.</param>
/// <param name="ProviderId">The identifier of the provider (null if anonymous).</param>
/// <param name="FeedbackType">The type of feedback.</param>
/// <param name="Feedback">The feedback text.</param>
/// <param name="IsAnonymous">Whether the feedback is anonymous.</param>
/// <param name="SubmittedDate">The date when feedback was submitted.</param>
public record FeedbackDto(
    Guid Id,
    Guid PerformanceReviewId,
    Guid? ProviderId,
    FeedbackType FeedbackType,
    string Feedback,
    bool IsAnonymous,
    DateTime SubmittedDate);
