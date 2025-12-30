using System.ComponentModel.DataAnnotations;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for updating the progress of a performance goal.
/// </summary>
public record UpdateGoalProgressRequest
{
    /// <summary>
    /// Gets or sets the progress update text.
    /// </summary>
    [Required]
    public string ProgressUpdate { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the new status of the goal.
    /// </summary>
    [Required]
    public GoalStatus CompletionStatus { get; init; }
}