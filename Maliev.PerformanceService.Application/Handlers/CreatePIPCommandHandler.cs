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
/// Handles creation of a new PIP.
/// </summary>
public class CreatePIPCommandHandler
{
    private readonly IPIPRepository _repository;
    private readonly IEmployeeServiceClient _employeeService;
    private readonly CreatePIPValidator _validator;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreatePIPCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePIPCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The PIP repository.</param>
    /// <param name="employeeService">The employee service client.</param>
    /// <param name="validator">The PIP validator.</param>
    /// <param name="publishEndpoint">The message bus publish endpoint.</param>
    /// <param name="logger">The logger.</param>
    public CreatePIPCommandHandler(
        IPIPRepository repository,
        IEmployeeServiceClient employeeService,
        CreatePIPValidator validator,
        IPublishEndpoint publishEndpoint,
        ILogger<CreatePIPCommandHandler> logger)
    {
        _repository = repository;
        _employeeService = employeeService;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Handles the creation of a new PIP.
    /// </summary>
    /// <param name="command">The creation command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(PerformanceImprovementPlan? PIP, string? Error)> HandleAsync(CreatePIPCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            return (null, validationResult.Error);
        }

        if (!await _employeeService.ValidateEmployeeExistsAsync(command.EmployeeId, cancellationToken))
        {
            return (null, "Employee not found.");
        }

        var activePip = await _repository.GetActivePIPByEmployeeIdAsync(command.EmployeeId, cancellationToken);
        if (activePip != null)
        {
            return (null, "Employee already has an active PIP.");
        }

        var pip = new PerformanceImprovementPlan
        {
            Id = Guid.NewGuid(),
            EmployeeId = command.EmployeeId,
            InitiatorId = command.RequestingUserId,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            Reason = command.Reason,
            ImprovementAreas = command.ImprovementAreas,
            SuccessCriteria = command.SuccessCriteria,
            Status = PIPStatus.Active,
            ExtensionCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        var createdPip = await _repository.CreateAsync(pip, cancellationToken);

        await _publishEndpoint.Publish(new PerformancePIPInitiatedEvent(
            createdPip.Id,
            createdPip.EmployeeId,
            createdPip.InitiatorId,
            createdPip.StartDate,
            createdPip.EndDate,
            createdPip.Reason), cancellationToken);

        _logger.LogInformation("PIP {PIPId} initiated for employee {EmployeeId} by {InitiatorId}.", 
            createdPip.Id, createdPip.EmployeeId, createdPip.InitiatorId);

        return (createdPip, null);
    }
}
