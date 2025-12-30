using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles updates to PIP check-in notes.
/// </summary>
public class UpdatePIPCommandHandler
{
    private readonly IPIPRepository _repository;
    private readonly ILogger<UpdatePIPCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePIPCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The PIP repository.</param>
    /// <param name="logger">The logger.</param>
    public UpdatePIPCommandHandler(IPIPRepository repository, ILogger<UpdatePIPCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Handles updating an existing PIP with check-in notes.
    /// </summary>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(PerformanceImprovementPlan? PIP, string? Error)> HandleAsync(UpdatePIPCommand command, CancellationToken cancellationToken = default)
    {
        var pip = await _repository.GetByIdAsync(command.PIPId, cancellationToken);
        if (pip == null)
        {
            return (null, "PIP not found.");
        }

        if (!string.IsNullOrWhiteSpace(command.CheckInNote))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var note = $"[{timestamp}] {command.CheckInNote}";
            pip.CheckInNotes = string.IsNullOrEmpty(pip.CheckInNotes)
                ? note
                : $"{pip.CheckInNotes}\n{note}";
        }

        pip.ModifiedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(pip, cancellationToken);

        _logger.LogInformation("PIP {PIPId} updated with new check-in notes.", pip.Id);

        return (pip, null);
    }
}
