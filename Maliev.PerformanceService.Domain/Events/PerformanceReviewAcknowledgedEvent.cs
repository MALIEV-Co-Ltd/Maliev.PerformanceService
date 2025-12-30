using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Events;

/// <summary>
/// Event produced when a performance review is formally acknowledged by the employee.
/// </summary>
/// <param name="ReviewId">The unique identifier of the review.</param>
/// <param name="EmployeeId">The identifier of the employee.</param>
/// <param name="OverallRating">The final rating assigned.</param>
/// <param name="AcknowledgedDate">The date when acknowledgement occurred.</param>
public record PerformanceReviewAcknowledgedEvent(
    Guid ReviewId,
    Guid EmployeeId,
    PerformanceRating? OverallRating,
    DateTime AcknowledgedDate);