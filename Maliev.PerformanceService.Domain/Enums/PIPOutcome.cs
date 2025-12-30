namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the outcome of a Performance Improvement Plan (PIP).
/// </summary>
public enum PIPOutcome
{
    /// <summary>
    /// The PIP was completed successfully and the employee met the criteria.
    /// </summary>
    Successful = 0,

    /// <summary>
    /// The PIP was not successful.
    /// </summary>
    Unsuccessful = 1,

    /// <summary>
    /// The PIP has been extended for another period.
    /// </summary>
    ExtendedAgain = 2
}