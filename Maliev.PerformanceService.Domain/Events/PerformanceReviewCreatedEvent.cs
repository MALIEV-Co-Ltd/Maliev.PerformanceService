using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a new performance review cycle is initiated.
/// </summary>
/// <param name="ReviewId">The unique identifier of the review.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="ReviewerId">The identifier of the assigned reviewer.</param>
/// <param name="ReviewCycle">The type of review cycle.</param>
/// <param name="ReviewPeriodStart">The start date of the review period.</param>
/// <param name="ReviewPeriodEnd">The end date of the review period.</param>
/// <param name="CreatedDate">The date when the review was created.</param>
public record PerformanceReviewCreatedEvent(
    Guid ReviewId,
    Guid EmployeeId,
    Guid ReviewerId,
    ReviewCycle ReviewCycle,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    DateTime CreatedDate);