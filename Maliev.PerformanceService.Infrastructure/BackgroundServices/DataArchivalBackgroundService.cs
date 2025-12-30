using Cronos;
using Maliev.PerformanceService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.BackgroundServices;

/// <summary>
/// Background service that periodically archives old performance data.
/// Runs monthly on the 1st day at midnight.
/// </summary>
public class DataArchivalBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataArchivalBackgroundService> _logger;
    private readonly CronExpression _cronExpression;

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
        // Monthly on the 1st at 00:00
        _cronExpression = CronExpression.Parse("0 0 1 * *");
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Data Archival Service starting...");

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

                await ArchiveOldDataAsync(stoppingToken);
            }
        }
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
