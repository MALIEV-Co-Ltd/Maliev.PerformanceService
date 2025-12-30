using System.ComponentModel.DataAnnotations;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for initiating a new performance review cycle.
/// </summary>
public record CreatePerformanceReviewRequest
{
    /// <summary>
    /// Gets or sets the review cycle type (e.g., Annual, Quarterly).
    /// </summary>
    [Required]
    public ReviewCycle ReviewCycle { get; init; }

    /// <summary>
    /// Gets or sets the start date of the review period.
    /// </summary>
    [Required]
    public DateTime ReviewPeriodStart { get; init; }

    /// <summary>
    /// Gets or sets the end date of the review period.
    /// </summary>
    [Required]
    public DateTime ReviewPeriodEnd { get; init; }

    /// <summary>
    /// Gets or sets optional initial self-assessment comments.
    /// </summary>
    [MaxLength(2000)]
    public string? SelfAssessment { get; init; }
}