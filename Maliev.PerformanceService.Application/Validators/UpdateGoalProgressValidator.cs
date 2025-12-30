using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Validators;

/// <summary>
/// Provides validation logic for updating goal progress.
/// </summary>
public class UpdateGoalProgressValidator
{
    /// <summary>
    /// Validates the progress update command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <param name="currentStatus">The current status of the goal.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public (bool IsValid, string? Error) Validate(UpdateGoalProgressCommand command, GoalStatus currentStatus)
    {
        if (string.IsNullOrWhiteSpace(command.ProgressUpdate))
        {
            return (false, "Progress update text is required.");
        }

        if (!IsValidTransition(currentStatus, command.CompletionStatus))
        {
            return (false, $"Invalid status transition from {currentStatus} to {command.CompletionStatus}.");
        }

        return (true, null);
    }

    private static bool IsValidTransition(GoalStatus from, GoalStatus to)
    {
        if (from == to) return true;

        return from switch
        {
            GoalStatus.NotStarted => to == GoalStatus.InProgress,
            GoalStatus.InProgress => to is GoalStatus.AtRisk or GoalStatus.Completed or GoalStatus.Deferred or GoalStatus.Cancelled,
            GoalStatus.AtRisk => to is GoalStatus.Completed or GoalStatus.Deferred or GoalStatus.Cancelled or GoalStatus.InProgress,
            GoalStatus.Completed => to == GoalStatus.InProgress,
            GoalStatus.Deferred => to is GoalStatus.InProgress or GoalStatus.Cancelled,
            GoalStatus.Cancelled => false,
            _ => false
        };
    }
}
