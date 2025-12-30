namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a performance goal is completed.
/// </summary>
/// <param name="GoalId">The unique identifier of the goal.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="Description">The description of the goal.</param>
/// <param name="CompletionDate">The date when the goal was marked as completed.</param>
public record GoalCompletedEvent(
    Guid GoalId,
    Guid EmployeeId,
    string Description,
    DateTime CompletionDate);