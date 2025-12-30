namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the frequency or type of a review cycle.
/// </summary>
public enum ReviewCycle
{
    /// <summary>
    /// Annual performance review.
    /// </summary>
    Annual = 0,

    /// <summary>
    /// Semi-annual performance review.
    /// </summary>
    SemiAnnual = 1,

    /// <summary>
    /// Quarterly performance review.
    /// </summary>
    Quarterly = 2,

    /// <summary>
    /// Probationary period review.
    /// </summary>
    Probation = 3
}