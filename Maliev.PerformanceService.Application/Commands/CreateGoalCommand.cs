using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to create a new performance goal for an employee.
/// </summary>
/// <param name="EmployeeId">The unique identifier of the employee.</param>
/// <param name="Description">The description of the goal.</param>
/// <param name="SuccessCriteria">The criteria for success.</param>
/// <param name="TargetCompletionDate">The scheduled completion date.</param>
/// <param name="PerformanceReviewId">The optional linked performance review identifier.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record CreateGoalCommand(
    Guid EmployeeId,
    string Description,
    string? SuccessCriteria,
    DateTime TargetCompletionDate,
    Guid? PerformanceReviewId,
    Guid RequestingUserId);