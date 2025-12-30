using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to record the outcome of a PIP.
/// </summary>
public record RecordPIPOutcomeCommand(
    Guid PIPId,
    PIPOutcome Outcome,
    DateTime? ExtendedEndDate,
    Guid RequestingUserId);
