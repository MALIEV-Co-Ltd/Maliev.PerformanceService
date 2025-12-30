namespace Maliev.PerformanceService.Application.Queries;

/// <summary>
/// Query to retrieve all performance reviews for a specific employee.
/// </summary>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record GetPerformanceReviewsQuery(
    Guid EmployeeId,
    Guid RequestingUserId);