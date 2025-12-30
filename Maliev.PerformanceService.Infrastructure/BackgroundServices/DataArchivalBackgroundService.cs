using Maliev.PerformanceService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically archives old performance data.
/// Runs monthly on the 1st day at midnight UTC.
/// </summary>
public class DataArchivalBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataArchivalBackgroundService> _logger;
    private readonly int _targetDayOfMonth = 1;
    private readonly TimeSpan _scheduledTime = TimeSpan.Zero; // Midnight

    /// <summary>
    /// Initializes a new instance of the <see cref="DataArchivalBackgroundService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger.</param>
    public DataArchivalBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DataArchivalBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Archival Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextRunTime = CalculateNextMonthlyRunTime(now, _targetDayOfMonth, _scheduledTime);
            var delay = nextRunTime - now;

            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, stoppingToken);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await ArchiveOldDataAsync(stoppingToken);
            }
        }
    }

    private static DateTime CalculateNextMonthlyRunTime(DateTime currentTime, int targetDay, TimeSpan targetTime)
    {
        var currentMonth = new DateTime(currentTime.Year, currentTime.Month, 1);
        var daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        var actualTargetDay = Math.Min(targetDay, daysInMonth);

        var nextTargetDate = new DateTime(currentMonth.Year, currentMonth.Month, actualTargetDay).Add(targetTime);

        // If we've already passed the target date this month, move to next month
        if (currentTime >= nextTargetDate)
        {
            var nextMonth = currentMonth.AddMonths(1);
            daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
            actualTargetDay = Math.Min(targetDay, daysInMonth);
            nextTargetDate = new DateTime(nextMonth.Year, nextMonth.Month, actualTargetDay).Add(targetTime);
        }

        return nextTargetDate;
    }

    private async Task ArchiveOldDataAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting data archival process...");

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPerformanceReviewRepository>();

        var sevenYearsAgo = DateTime.UtcNow.AddYears(-7);
        var oldReviews = await repository.GetReviewsForArchivalAsync(sevenYearsAgo, stoppingToken);

        int count = 0;
        foreach (var review in oldReviews)
        {
            try
            {
                // In a real system, we might move this to Google Cloud Storage first.
                // For this implementation, we just mark it as archived in the DB.
                await repository.MarkAsArchivedAsync(review.Id, stoppingToken);
                count++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving review {ReviewId}.", review.Id);
            }
        }

        _logger.LogInformation("Archived {Count} reviews older than 7 years.", count);
    }
}
