namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve a paginated list of goals for a specific employee.
/// </summary>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="Cursor">The cursor for pagination.</param>
/// <param name="Limit">The maximum number of items to return.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record GetGoalsQuery(
    Guid EmployeeId,
    Guid? Cursor,
    int Limit,
    Guid RequestingUserId);