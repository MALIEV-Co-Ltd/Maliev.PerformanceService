using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Api.DTOs;

/// <summary>
/// Data transfer object for Performance Improvement Plan (PIP).
/// </summary>
public record PIPDto(
    Guid Id,
    Guid EmployeeId,
    Guid InitiatorId,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    string ImprovementAreas,
    string SuccessCriteria,
    string? CheckInNotes,
    PIPStatus Status,
    PIPOutcome? Outcome,
    int ExtensionCount);
