namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Request DTO for updating PIP check-in notes.
/// </summary>
public record UpdatePIPRequest
{
    /// <summary>
    /// Gets or sets the check-in note to add.
    /// </summary>
    public string? CheckInNote { get; init; }
}