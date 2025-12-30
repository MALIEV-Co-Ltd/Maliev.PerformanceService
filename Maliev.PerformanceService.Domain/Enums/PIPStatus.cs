namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the status of a Performance Improvement Plan (PIP).
/// </summary>
public enum PIPStatus
{
    /// <summary>
    /// The PIP is currently active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The PIP has been extended.
    /// </summary>
    Extended = 1,

    /// <summary>
    /// The PIP has been completed.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// The PIP has been terminated (e.g. employee left).
    /// </summary>
    Terminated = 3
}