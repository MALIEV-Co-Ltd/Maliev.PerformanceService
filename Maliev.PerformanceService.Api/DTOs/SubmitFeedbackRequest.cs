using System.ComponentModel.DataAnnotations;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for submitting performance feedback.
/// </summary>
public record SubmitFeedbackRequest
{
    /// <summary>
    /// Gets or sets the type of feedback.
    /// </summary>
    [Required]
    public FeedbackType FeedbackType { get; init; }

    /// <summary>
    /// Gets or sets the feedback narrative.
    /// </summary>
    [Required]
    public string Feedback { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the feedback should be anonymous.
    /// </summary>
    public bool IsAnonymous { get; init; }
}
