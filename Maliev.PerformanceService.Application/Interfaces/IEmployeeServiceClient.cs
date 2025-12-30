namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Client for interacting with the Employee microservice.
/// </summary>
public interface IEmployeeServiceClient
{
    /// <summary>
    /// Gets basic employee details by their unique identifier.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Employee details or null if not found.</returns>
    Task<EmployeeDto?> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether an employee exists in the system.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the employee exists, otherwise false.</returns>
    Task<bool> ValidateEmployeeExistsAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a specific manager is assigned to an employee.
    /// </summary>
    /// <param name="managerId">The manager identifier.</param>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the relationship is valid, otherwise false.</returns>
    Task<bool> ValidateManagerEmployeeRelationshipAsync(Guid managerId, Guid employeeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Basic employee data transfer object for inter-service communication.
/// </summary>
/// <param name="EmployeeId">The employee identifier.</param>
/// <param name="FullName">The full name of the employee.</param>
/// <param name="Email">The primary email address.</param>
/// <param name="ManagerId">The identifier of the assigned manager.</param>
public record EmployeeDto(Guid EmployeeId, string FullName, string Email, Guid? ManagerId);