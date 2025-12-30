using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the creation of a new performance goal.
/// </summary>
public class CreateGoalCommandHandler
{
    private readonly IGoalRepository _repository;
    private readonly IEmployeeServiceClient _employeeService;
    private readonly INotificationServiceClient _notificationService;
    private readonly CreateGoalValidator _validator;
    private readonly ILogger<CreateGoalCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateGoalCommandHandler"/> class.
    /// </summary>
    public CreateGoalCommandHandler(
        IGoalRepository repository,
        IEmployeeServiceClient employeeService,
        INotificationServiceClient notificationService,
        CreateGoalValidator validator,
        ILogger<CreateGoalCommandHandler> logger)
    {
        _repository = repository;
        _employeeService = employeeService;
        _notificationService = notificationService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Executes the creation of a performance goal.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created goal or an error message.</returns>
    public async Task<(Goal? Goal, string? Error)> HandleAsync(CreateGoalCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for goal creation: {Error}", validationResult.Error);
            return (null, validationResult.Error);
        }

        if (!await _employeeService.ValidateEmployeeExistsAsync(command.EmployeeId, cancellationToken))
        {
            _logger.LogWarning("Employee {EmployeeId} not found for goal creation.", command.EmployeeId);
            return (null, "Employee not found.");
        }

        // Data Volume Limits
        var count = await _repository.CountByEmployeeIdAsync(command.EmployeeId, cancellationToken);
        if (count >= 100)
        {
            return (null, "DATA_VOLUME_LIMIT_REACHED: Maximum of 100 goals per employee.");
        }
        if (count >= 80)
        {
            await _notificationService.SendDataVolumeWarningAsync(command.EmployeeId, "Goal", count, 100, cancellationToken);
        }

        var goal = new Goal
        {
            Id = Guid.NewGuid(),
            EmployeeId = command.EmployeeId,
            PerformanceReviewId = command.PerformanceReviewId,
            Description = command.Description,
            SuccessCriteria = command.SuccessCriteria,
            TargetCompletionDate = command.TargetCompletionDate,
            CurrentStatus = GoalStatus.NotStarted,
            CreatedDate = DateTime.UtcNow
        };

        var createdGoal = await _repository.CreateAsync(goal, cancellationToken);
        _logger.LogInformation("Goal {GoalId} created for employee {EmployeeId}.", createdGoal.Id, createdGoal.EmployeeId);

        return (createdGoal, null);
    }
}
