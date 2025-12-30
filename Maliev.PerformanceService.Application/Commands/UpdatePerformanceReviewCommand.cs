namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to update a performance review, typically for saving draft comments.
/// </summary>
/// <param name="ReviewId">The identifier of the review.</param>
/// <param name="SelfAssessment">The updated self-assessment comments.</param>
/// <param name="ManagerAssessment">The updated manager assessment comments.</param>
/// <param name="SubmitSelfAssessment">A value indicating whether to formally submit the self-assessment.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record UpdatePerformanceReviewCommand(
    Guid ReviewId,
    string? SelfAssessment,
    string? ManagerAssessment,
    bool SubmitSelfAssessment,
    Guid RequestingUserId);