namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the rating scale for performance reviews.
/// </summary>
public enum PerformanceRating
{
    /// <summary>
    /// Performance does not meet the requirements of the job.
    /// </summary>
    Unsatisfactory = 1,

    /// <summary>
    /// Performance is below expectations and requires improvement.
    /// </summary>
    BelowExpectations = 2,

    /// <summary>
    /// Performance meets the requirements and expectations of the job.
    /// </summary>
    NeedsImprovement = 3,

    /// <summary>
    /// Performance meets all requirements and expectations.
    /// </summary>
    MeetsExpectations = 4,

    /// <summary>
    /// Performance consistently exceeds requirements and expectations.
    /// </summary>
    ExceedsExpectations = 5
}