namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a Performance Improvement Plan (PIP) is initiated.
/// </summary>
/// <param name="PIPId">The unique identifier of the PIP.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="InitiatorId">The identifier of the person who initiated the PIP.</param>
/// <param name="StartDate">The start date of the PIP.</param>
/// <param name="EndDate">The scheduled end date of the PIP.</param>
/// <param name="Reason">The reason why the PIP was initiated.</param>
public record PIPInitiatedEvent(
    Guid PIPId,
    Guid EmployeeId,
    Guid InitiatorId,
    DateTime StartDate,
    DateTime EndDate,
    string Reason);