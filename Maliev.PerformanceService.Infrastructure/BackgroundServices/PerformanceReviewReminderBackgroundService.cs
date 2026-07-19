using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically sends reminders for pending performance reviews.
/// Runs daily at 8 AM UTC.
/// </summary>
public class PerformanceReviewReminderBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PerformanceReviewReminderBackgroundService> _logger;
    private readonly TimeSpan _scheduledTime = new(8, 0, 0); // 8:00 AM

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewReminderBackgroundService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public PerformanceReviewReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PerformanceReviewReminderBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Performance Review Reminder Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = CalculateNextDailyRunTime(now, _scheduledTime);
            var delay = nextRunTime - now;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await SendRemindersAsync(stoppingToken);
            }
        }
    }

    private static DateTime CalculateNextDailyRunTime(DateTime currentTime, TimeSpan targetTime)
    {
        var today = currentTime.Date.Add(targetTime);
        return currentTime < today ? today : today.AddDays(1);
    }

    private async Task SendRemindersAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Sending performance review reminders...");

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPerformanceReviewRepository>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationServiceClient>();

        var pendingReviews = await repository.GetPendingReviewsAsync(stoppingToken);

        foreach (var review in pendingReviews)
        {
            try
            {
                if (review.Status == ReviewStatus.SelfAssessmentPending)
                {
                    await notificationService.SendReviewReminderAsync(review.EmployeeId, review.Id, "SelfAssessment", stoppingToken);
                }
                else if (review.Status == ReviewStatus.ManagerReviewPending)
                {
                    await notificationService.SendReviewReminderAsync(review.ReviewerId, review.Id, "ManagerReview", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for review {ReviewId}.", review.Id);
            }
        }

        _logger.LogInformation("Finished sending performance review reminders.");
    }
}
