using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Entities;

/// <summary>
/// Represents a performance goal set for an employee.
/// </summary>
public class Goal
{
    /// <summary>
    /// Gets or sets the unique identifier for the goal.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the employee this goal belongs to.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the performance review this goal is associated with, if any.
    /// </summary>
    public Guid? PerformanceReviewId { get; set; }

    /// <summary>
    /// Gets or sets the description of the goal.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the success criteria for achieving the goal.
    /// </summary>
    public string? SuccessCriteria { get; set; }

    /// <summary>
    /// Gets or sets the target completion date for the goal.
    /// </summary>
    public DateTime TargetCompletionDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the goal.
    /// </summary>
    public GoalStatus CurrentStatus { get; set; }

    /// <summary>
    /// Gets or sets timestamped progress updates for the goal.
    /// </summary>
    public string? ProgressUpdates { get; set; }

    /// <summary>
    /// Gets or sets the date when the goal was completed.
    /// </summary>
    public DateTime? CompletionDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the goal was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the goal was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}