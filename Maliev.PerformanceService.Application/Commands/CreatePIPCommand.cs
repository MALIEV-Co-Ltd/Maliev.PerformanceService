namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to create a new Performance Improvement Plan.
/// </summary>
public record CreatePIPCommand(
    Guid EmployeeId,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    string ImprovementAreas,
    string SuccessCriteria,
    Guid RequestingUserId);
