namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when an employee is terminated.
/// Consumed from Employee Service per MessagingContracts.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="TerminationDate">The date when the employment ended.</param>
/// <param name="TerminationReason">The reason for termination.</param>
/// <param name="EligibleForRehire">A value indicating if the employee is eligible for rehire.</param>
public record EmployeeTerminatedEvent(
    Guid EmployeeId,
    DateTime TerminationDate,
    string? TerminationReason,
    bool EligibleForRehire);