using System.ComponentModel.DataAnnotations;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for updating a performance review (e.g., saving a draft self-assessment).
/// </summary>
public record UpdatePerformanceReviewRequest
{
    /// <summary>
    /// Gets or sets updated self-assessment comments.
    /// </summary>
    [MaxLength(2000)]
    public string? SelfAssessment { get; init; }

    /// <summary>
    /// Gets or sets updated manager assessment comments.
    /// </summary>
    [MaxLength(2000)]
    public string? ManagerAssessment { get; init; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to formally submit the self-assessment.
    /// </summary>
    public bool SubmitSelfAssessment { get; init; }
}