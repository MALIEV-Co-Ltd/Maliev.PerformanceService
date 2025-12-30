using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.Consumers;

/// <summary>
/// Consumes EmployeeTerminatedEvent and closes active performance data.
/// </summary>
public class EmployeeTerminatedEventConsumer : IConsumer<EmployeeTerminatedEvent>
{
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly IPIPRepository _pipRepository;
    private readonly ILogger<EmployeeTerminatedEventConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeTerminatedEventConsumer"/> class.
    /// </summary>
    /// <param name="reviewRepository">The performance review repository.</param>
    /// <param name="pipRepository">The PIP repository.</param>
    /// <param name="logger">The logger.</param>
    public EmployeeTerminatedEventConsumer(
        IPerformanceReviewRepository reviewRepository,
        IPIPRepository pipRepository,
        ILogger<EmployeeTerminatedEventConsumer> logger)
    {
        _reviewRepository = reviewRepository;
        _pipRepository = pipRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Consume(ConsumeContext<EmployeeTerminatedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing termination for employee {EmployeeId}.", message.EmployeeId);

        // 1. Close active reviews
        var reviews = await _reviewRepository.GetByEmployeeIdAsync(message.EmployeeId);
        foreach (var review in reviews.Where(r => r.Status != ReviewStatus.Completed))
        {
            review.Status = ReviewStatus.Completed; // Mark as closed early
            review.ModifiedDate = DateTime.UtcNow;
            await _reviewRepository.UpdateAsync(review);
            _logger.LogInformation("Review {ReviewId} closed early due to termination.", review.Id);
        }

        // 2. Terminate active PIPs
        var activePip = await _pipRepository.GetActivePIPByEmployeeIdAsync(message.EmployeeId);
        if (activePip != null)
        {
            activePip.Status = PIPStatus.Terminated;
            activePip.Outcome = PIPOutcome.Unsuccessful; // Default for termination
            activePip.ModifiedDate = DateTime.UtcNow;
            await _pipRepository.UpdateAsync(activePip);
            _logger.LogInformation("PIP {PIPId} terminated due to employee termination.", activePip.Id);
        }
    }
}
