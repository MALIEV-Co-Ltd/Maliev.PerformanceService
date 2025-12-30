namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the possible statuses of a performance goal.
/// </summary>
public enum GoalStatus
{
    /// <summary>
    /// The goal has not yet been started.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// The goal is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// The goal is at risk of not being completed on time.
    /// </summary>
    AtRisk = 2,

    /// <summary>
    /// The goal has been successfully completed.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// The goal has been deferred to a later period.
    /// </summary>
    Deferred = 4,

    /// <summary>
    /// The goal has been cancelled.
    /// </summary>
    Cancelled = 5
}