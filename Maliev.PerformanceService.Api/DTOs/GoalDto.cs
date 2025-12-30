using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Data transfer object for performance goal details.
/// </summary>
/// <param name="Id">The unique identifier of the goal.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="PerformanceReviewId">The identifier of the linked performance review, if any.</param>
/// <param name="Description">The description of the goal.</param>
/// <param name="SuccessCriteria">The success criteria.</param>
/// <param name="TargetCompletionDate">The target completion date.</param>
/// <param name="CurrentStatus">The current status of the goal.</param>
/// <param name="ProgressUpdates">Timestamped progress updates.</param>
/// <param name="CompletionDate">The date when the goal was completed.</param>
public record GoalDto(
    Guid Id,
    Guid EmployeeId,
    Guid? PerformanceReviewId,
    string Description,
    string? SuccessCriteria,
    DateTime TargetCompletionDate,
    GoalStatus CurrentStatus,
    string? ProgressUpdates,
    DateTime? CompletionDate);