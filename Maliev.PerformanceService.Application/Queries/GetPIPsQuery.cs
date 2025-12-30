namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve PIPs for an employee.
/// </summary>
public record GetPIPsQuery(Guid EmployeeId, Guid RequestingUserId);
