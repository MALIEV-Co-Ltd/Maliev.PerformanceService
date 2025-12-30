namespace Maliev.PerformanceService.Domain.Entities;

/// <summary>
/// Represents a formal disciplinary action taken against an employee.
/// </summary>
public class DisciplinaryAction
{
    /// <summary>
    /// Gets or sets the unique identifier for the disciplinary action.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the employee the action is against.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the person (manager/HR) who initiated the action.
    /// </summary>
    public Guid InitiatorId { get; set; }

    /// <summary>
    /// Gets or sets the date when the action was recorded.
    /// </summary>
    public DateTime ActionDate { get; set; }

    /// <summary>
    /// Gets or sets the type of disciplinary action (e.g., Verbal Warning, Written Warning).
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed reason for the disciplinary action.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the duration for which the action remains active on the employee's record.
    /// </summary>
    public string? ValidityPeriod { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }
}
