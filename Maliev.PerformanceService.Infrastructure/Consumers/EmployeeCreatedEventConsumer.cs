using Maliev.MessagingContracts.Contracts.Employee;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeCreatedEvent from the Employee Service.
/// </summary>
public class EmployeeCreatedEventConsumer : IConsumer<EmployeeCreatedEvent>
{
    private readonly ILogger<EmployeeCreatedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeCreatedEventConsumer"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public EmployeeCreatedEventConsumer(ILogger<EmployeeCreatedEventConsumer> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task Consume(ConsumeContext<EmployeeCreatedEvent> context)
    {
        var payload = context.Message.Payload;
        _logger.LogInformation("Employee created: {EmployeeId}, EmployeeNumber: {EmployeeNumber}.",
            payload.EmployeeId, payload.EmployeeNumber);

        // Additional logic like caching or initializing settings can be added here.

        return Task.CompletedTask;
    }
}

