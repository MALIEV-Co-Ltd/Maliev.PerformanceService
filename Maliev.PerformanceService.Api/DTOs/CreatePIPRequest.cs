using System.ComponentModel.DataAnnotations;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for creating a new PIP.
/// </summary>
public record CreatePIPRequest
{
    /// <summary>
    /// Gets or sets the employee identifier.
    /// </summary>
    [Required]
    public Guid EmployeeId { get; init; }

    /// <summary>
    /// Gets or sets the start date of the PIP.
    /// </summary>
    [Required]
    public DateTime StartDate { get; init; }

    /// <summary>
    /// Gets or sets the end date of the PIP.
    /// </summary>
    [Required]
    public DateTime EndDate { get; init; }

    /// <summary>
    /// Gets or sets the reason for the PIP.
    /// </summary>
    [Required]
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the areas for improvement.
    /// </summary>
    [Required]
    public string ImprovementAreas { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the criteria for success.
    /// </summary>
    [Required]
    public string SuccessCriteria { get; init; } = string.Empty;
}