namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve a specific performance goal by its identifier.
/// </summary>
/// <param name="GoalId">The identifier of the goal.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record GetGoalByIdQuery(
    Guid GoalId,
    Guid RequestingUserId);