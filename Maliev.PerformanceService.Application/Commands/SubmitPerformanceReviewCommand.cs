using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command for a manager to formally submit a performance review.
/// </summary>
/// <param name="ReviewId">The identifier of the review to submit.</param>
/// <param name="ManagerAssessment">The manager's assessment comments.</param>
/// <param name="OverallRating">The overall performance rating.</param>
/// <param name="RequestingUserId">The identifier of the user making the request (must be the manager).</param>
public record SubmitPerformanceReviewCommand(
    Guid ReviewId,
    string ManagerAssessment,
    PerformanceRating OverallRating,
    Guid RequestingUserId);