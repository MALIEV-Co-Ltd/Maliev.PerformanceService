using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles updates to existing performance goals.
/// </summary>
public class UpdateGoalCommandHandler
{
    private readonly IGoalRepository _repository;
    private readonly ILogger<UpdateGoalCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateGoalCommandHandler"/> class.
    /// </summary>
    public UpdateGoalCommandHandler(IGoalRepository repository, ILogger<UpdateGoalCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Executes an update to a performance goal record.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated goal or an error message.</returns>
    public async Task<(Goal? Goal, string? Error)> HandleAsync(UpdateGoalCommand command, CancellationToken cancellationToken = default)
    {
        var goal = await _repository.GetByIdAsync(command.GoalId, cancellationToken);
        if (goal == null)
        {
            _logger.LogWarning("Goal {GoalId} not found for update.", command.GoalId);
            return (null, "Goal not found.");
        }

        goal.Description = command.Description;
        goal.SuccessCriteria = command.SuccessCriteria;
        goal.TargetCompletionDate = command.TargetCompletionDate;
        goal.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(goal, cancellationToken);
        _logger.LogInformation("Goal {GoalId} details updated.", goal.Id);

        return (goal, null);
    }
}
