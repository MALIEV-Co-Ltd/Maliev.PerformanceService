namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a new employee is created in the system.
/// Consumed from Employee Service per MessagingContracts.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="EmployeeNumber">The unique business number of the employee.</param>
/// <param name="StartDate">The employee's starting date.</param>
/// <param name="DepartmentId">The identifier of the department the employee belongs to.</param>
/// <param name="PositionId">The identifier of the employee's position.</param>
/// <param name="ManagerId">The identifier of the employee's manager.</param>
public record EmployeeCreatedEvent(
    Guid EmployeeId,
    string EmployeeNumber,
    DateTime StartDate,
    Guid DepartmentId,
    Guid? PositionId,
    Guid? ManagerId);