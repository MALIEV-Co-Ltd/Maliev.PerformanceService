using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a Performance Improvement Plan (PIP) is completed.
/// </summary>
/// <param name="PIPId">The unique identifier of the PIP.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="Outcome">The final outcome of the PIP.</param>
/// <param name="CompletedDate">The date when the PIP process ended.</param>
public record PIPCompletedEvent(
    Guid PIPId,
    Guid EmployeeId,
    PIPOutcome? Outcome,
    DateTime CompletedDate);