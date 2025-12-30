using Cronos;
using Maliev.PerformanceService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically sends reminders for PIP check-ins.
/// Runs weekly on Monday at 9 AM.
/// </summary>
public class PIPCheckInReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PIPCheckInReminderBackgroundService> _logger;
    private readonly CronExpression _cronExpression;

    /// <summary>
    /// Initializes a new instance of the <see cref="PIPCheckInReminderBackgroundService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public PIPCheckInReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PIPCheckInReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        // Weekly on Monday at 9:00 AM
        _cronExpression = CronExpression.Parse("0 9 * * 1");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PIP Check-In Reminder Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var utcNow = DateTime.UtcNow;
            var nextUtc = _cronExpression.GetNextOccurrence(utcNow);

            if (nextUtc.HasValue)
            {
                var delay = nextUtc.Value - utcNow;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, stoppingToken);
                }

                await SendRemindersAsync(stoppingToken);
            }
        }
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sending PIP check-in reminders...");

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPIPRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

        var activePips = await repository.GetAllActivePIPsAsync(stoppingToken);

        foreach (var pip in activePips)
        {
            try
            {
                // Simple reminder logic: notify about check-in for the current week
                await notificationService.SendPIPCheckInReminderAsync(pip.EmployeeId, pip.Id, DateTime.UtcNow, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending PIP reminder for {PIPId}.", pip.Id);
            }
        }

        _logger.LogInformation("Finished sending PIP check-in reminders.");
    }
}
