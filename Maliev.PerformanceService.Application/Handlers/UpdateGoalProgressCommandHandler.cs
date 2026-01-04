using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles updates to the progress and status of a performance goal.
/// </summary>
public class UpdateGoalProgressCommandHandler
{
    private readonly IGoalRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly INotificationServiceClient _notificationService;
    private readonly UpdateGoalProgressValidator _validator;
    private readonly ILogger<UpdateGoalProgressCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateGoalProgressCommandHandler"/> class.
    /// </summary>
    public UpdateGoalProgressCommandHandler(
        IGoalRepository repository, 
        IPublishEndpoint publishEndpoint,
        INotificationServiceClient notificationService,
        UpdateGoalProgressValidator validator,
        ILogger<UpdateGoalProgressCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _notificationService = notificationService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Executes a progress update for a goal.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated goal or an error message.</returns>
    public async Task<(Goal? Goal, string? Error)> HandleAsync(UpdateGoalProgressCommand command, CancellationToken cancellationToken = default)
    {
        var goal = await _repository.GetByIdAsync(command.GoalId, cancellationToken);
        if (goal == null)
        {
            _logger.LogWarning("Goal {GoalId} not found for progress update.", command.GoalId);
            return (null, "Goal not found.");
        }

        var validationResult = _validator.Validate(command, goal.CurrentStatus);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for goal {GoalId} progress update: {Error}", command.GoalId, validationResult.Error);
            return (null, validationResult.Error);
        }

        var oldStatus = goal.CurrentStatus;
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        goal.ProgressUpdates = string.IsNullOrEmpty(goal.ProgressUpdates) 
            ? "[" + timestamp + "] " + command.ProgressUpdate 
            : $"{goal.ProgressUpdates}\n[" + timestamp + "] " + command.ProgressUpdate;

        goal.CurrentStatus = command.CompletionStatus;
        goal.ModifiedDate = DateTime.UtcNow;

        if (command.CompletionStatus == GoalStatus.Completed && oldStatus != GoalStatus.Completed)
        {
            goal.CompletionDate = DateTime.UtcNow;
            
            await _publishEndpoint.Publish(new PerformanceGoalCompletedEvent(
                goal.Id,
                goal.EmployeeId,
                goal.Description,
                goal.CompletionDate.Value), cancellationToken);

            _logger.LogInformation("Goal {GoalId} marked as completed for employee {EmployeeId}.", goal.Id, goal.EmployeeId);
        }

        if (command.CompletionStatus == GoalStatus.AtRisk && oldStatus != GoalStatus.AtRisk)
        {
            await _notificationService.SendGoalAtRiskAlertAsync(goal.EmployeeId, goal.Id, cancellationToken);
            _logger.LogInformation("AtRisk alert sent for goal {GoalId} (employee {EmployeeId}).", goal.Id, goal.EmployeeId);
        }

        await _repository.UpdateAsync(goal, cancellationToken);

        _logger.LogInformation("Progress updated for goal {GoalId}. Status changed from {OldStatus} to {NewStatus}.", 
            goal.Id, oldStatus, goal.CurrentStatus);

        return (goal, null);
    }
}