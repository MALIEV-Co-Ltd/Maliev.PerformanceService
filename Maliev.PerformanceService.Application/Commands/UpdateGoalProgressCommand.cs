using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to update the progress of a performance goal.
/// </summary>
/// <param name="GoalId">The identifier of the goal.</param>
/// <param name="ProgressUpdate">The progress update text.</param>
/// <param name="CompletionStatus">The current status of the goal.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record UpdateGoalProgressCommand(
    Guid GoalId,
    string ProgressUpdate,
    GoalStatus CompletionStatus,
    Guid RequestingUserId);