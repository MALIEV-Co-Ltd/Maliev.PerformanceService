using System.ComponentModel.DataAnnotations;
using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for recording PIP outcome.
/// </summary>
public record RecordPIPOutcomeRequest
{
    /// <summary>
    /// Gets or sets the outcome of the PIP.
    /// </summary>
    [Required]
    public PIPOutcome Outcome { get; init; }

    /// <summary>
    /// Gets or sets the optional extended end date if the PIP is extended.
    /// </summary>
    public DateTime? ExtendedEndDate { get; init; }
}