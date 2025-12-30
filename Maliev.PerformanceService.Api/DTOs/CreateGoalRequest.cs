using System.ComponentModel.DataAnnotations;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for creating a new performance goal.
/// </summary>
public record CreateGoalRequest
{
    /// <summary>
    /// Gets or sets the description of the goal.
    /// </summary>
    [Required]
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the success criteria for the goal.
    /// </summary>
    public string? SuccessCriteria { get; init; }

    /// <summary>
    /// Gets or sets the target completion date. Must be in the future.
    /// </summary>
    [Required]
    public DateTime TargetCompletionDate { get; init; }

    /// <summary>
    /// Gets or sets an optional performance review identifier to link this goal to.
    /// </summary>
    public Guid? PerformanceReviewId { get; init; }
}