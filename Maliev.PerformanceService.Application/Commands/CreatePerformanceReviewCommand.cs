using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to create a new performance review cycle.
/// </summary>
/// <param name="EmployeeId">The identifier of the employee to review.</param>
/// <param name="ReviewCycle">The type of review cycle.</param>
/// <param name="ReviewPeriodStart">The start date of the period.</param>
/// <param name="ReviewPeriodEnd">The end date of the period.</param>
/// <param name="SelfAssessment">The optional initial self-assessment comments.</param>
/// <param name="RequestingUserId">The identifier of the user making the request.</param>
public record CreatePerformanceReviewCommand(
    Guid EmployeeId,
    ReviewCycle ReviewCycle,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    string? SelfAssessment,
    Guid RequestingUserId);