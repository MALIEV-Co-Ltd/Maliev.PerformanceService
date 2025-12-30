using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Entities;

/// <summary>
/// Represents a Performance Improvement Plan (PIP) for an employee.
/// </summary>
public class PerformanceImprovementPlan
{
    /// <summary>
    /// Gets or sets the unique identifier for the PIP.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the employee this PIP applies to.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the person (manager/HR) who initiated the PIP.
    /// </summary>
    public Guid InitiatorId { get; set; }

    /// <summary>
    /// Gets or sets the start date of the PIP.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the scheduled end date of the PIP.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets or sets the reason for initiating the PIP.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the specific areas requiring improvement.
    /// </summary>
    public string ImprovementAreas { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the criteria for successful completion of the PIP.
    /// </summary>
    public string SuccessCriteria { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets timestamped notes from PIP check-in meetings.
    /// </summary>
    public string? CheckInNotes { get; set; }

    /// <summary>
    /// Gets or sets the current status of the PIP.
    /// </summary>
    public PIPStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the final outcome of the PIP.
    /// </summary>
    public PIPOutcome? Outcome { get; set; }

    /// <summary>
    /// Gets or sets how many times this PIP has been extended.
    /// </summary>
    public int ExtensionCount { get; set; }

    /// <summary>
    /// Gets or sets the date when the PIP was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the PIP was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}