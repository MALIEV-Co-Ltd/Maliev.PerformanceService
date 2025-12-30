using System.ComponentModel.DataAnnotations;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for a manager to submit their assessment and rating for a review.
/// </summary>
public record SubmitPerformanceReviewRequest
{
    /// <summary>
    /// Gets or sets the manager's assessment comments.
    /// </summary>
    [Required]
    public string ManagerAssessment { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the overall performance rating assigned by the manager.
    /// </summary>
    [Required]
    public PerformanceRating OverallRating { get; init; }
}