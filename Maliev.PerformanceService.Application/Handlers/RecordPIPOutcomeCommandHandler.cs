using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles recording the outcome of a PIP.
/// </summary>
public class RecordPIPOutcomeCommandHandler
{
    private readonly IPIPRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RecordPIPOutcomeCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RecordPIPOutcomeCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The PIP repository.</param>
    /// <param name="publishEndpoint">The message bus publish endpoint.</param>
    /// <param name="logger">The logger.</param>
    public RecordPIPOutcomeCommandHandler(
        IPIPRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<RecordPIPOutcomeCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Handles recording the outcome of a PIP.
    /// </summary>
    /// <param name="command">The outcome command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(PerformanceImprovementPlan? PIP, string? Error)> HandleAsync(RecordPIPOutcomeCommand command, CancellationToken cancellationToken = default)
    {
        var pip = await _repository.GetByIdAsync(command.PIPId, cancellationToken);
        if (pip == null)
        {
            return (null, "PIP not found.");
        }

        if (pip.Status is PIPStatus.Completed or PIPStatus.Terminated)
        {
            return (null, "PIP outcome has already been recorded.");
        }

        if (command.Outcome == PIPOutcome.ExtendedAgain)
        {
            if (pip.ExtensionCount >= 1)
            {
                return (null, "Maximum of one extension is allowed for PIP.");
            }

            if (command.ExtendedEndDate == null || command.ExtendedEndDate <= pip.EndDate)
            {
                return (null, "A valid future extension end date is required.");
            }

            pip.Status = PIPStatus.Extended;
            pip.EndDate = command.ExtendedEndDate.Value;
            pip.ExtensionCount++;
        }
        else
        {
            pip.Status = command.Outcome == PIPOutcome.Successful ? PIPStatus.Completed : PIPStatus.Terminated;
            pip.Outcome = command.Outcome;
            
            await _publishEndpoint.Publish(new PIPCompletedEvent(
                pip.Id,
                pip.EmployeeId,
                pip.Outcome,
                DateTime.UtcNow), cancellationToken);
        }

        pip.ModifiedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(pip, cancellationToken);

        _logger.LogInformation("Outcome {Outcome} recorded for PIP {PIPId}. New Status: {Status}.", 
            command.Outcome, pip.Id, pip.Status);

        return (pip, null);
    }
}
