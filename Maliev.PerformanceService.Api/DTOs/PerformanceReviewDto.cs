using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Data transfer object for performance review details.
/// </summary>
/// <param name="Id">The unique identifier of the review.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="ReviewerId">The identifier of the reviewer.</param>
/// <param name="ReviewCycle">The review cycle type.</param>
/// <param name="ReviewPeriodStart">The start date of the period.</param>
/// <param name="ReviewPeriodEnd">The end date of the period.</param>
/// <param name="SelfAssessment">The employee's self-assessment comments.</param>
/// <param name="ManagerAssessment">The manager's assessment comments.</param>
/// <param name="OverallRating">The assigned overall rating.</param>
/// <param name="Status">The current status of the review.</param>
/// <param name="SubmittedDate">The date when submitted by the manager.</param>
/// <param name="AcknowledgedDate">The date when acknowledged by the employee.</param>
public record PerformanceReviewDto(
    Guid Id,
    Guid EmployeeId,
    Guid ReviewerId,
    ReviewCycle ReviewCycle,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    string? SelfAssessment,
    string? ManagerAssessment,
    PerformanceRating? OverallRating,
    ReviewStatus Status,
    DateTime? SubmittedDate,
    DateTime? AcknowledgedDate);