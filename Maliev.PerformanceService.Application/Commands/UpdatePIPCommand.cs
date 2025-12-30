namespace Maliev.PerformanceService.Application.Commands;

/// <summary>
/// Command to update PIP check-in notes.
/// </summary>
public record UpdatePIPCommand(
    Guid PIPId,
    string CheckInNote,
    Guid RequestingUserId);
