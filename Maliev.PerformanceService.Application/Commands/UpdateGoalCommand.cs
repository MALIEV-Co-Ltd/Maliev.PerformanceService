namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to update the details of an existing goal.
/// </summary>
/// <param name="GoalId">The identifier of the goal to update.</param>
/// <param name="Description">The updated description.</param>
/// <param name="SuccessCriteria">The updated success criteria.</param>
/// <param name="TargetCompletionDate">The updated target completion date.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record UpdateGoalCommand(
    Guid GoalId,
    string Description,
    string? SuccessCriteria,
    DateTime TargetCompletionDate,
    Guid RequestingUserId);