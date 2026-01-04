using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handler for AcknowledgePerformanceReviewCommand.
/// </summary>
public class AcknowledgePerformanceReviewCommandHandler
{
    private readonly IPerformanceReviewRepository _repository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AcknowledgePerformanceReviewCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AcknowledgePerformanceReviewCommandHandler"/> class.
    /// </summary>
    /// <param name="repository">The performance review repository.</param>
    /// <param name="publishEndpoint">The message bus publish endpoint.</param>
    /// <param name="logger">The logger.</param>
    public AcknowledgePerformanceReviewCommandHandler(
        IPerformanceReviewRepository repository,
        IPublishEndpoint publishEndpoint,
        ILogger<AcknowledgePerformanceReviewCommandHandler> logger)
    {
        _repository = repository;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <summary>
    /// Handles the acknowledgment of a performance review.
    /// </summary>
    /// <param name="command">The acknowledgment command.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(PerformanceReview? Review, string? Error)> HandleAsync(AcknowledgePerformanceReviewCommand command)
    {
        var review = await _repository.GetByIdAsync(command.ReviewId);
        if (review == null)
        {
            _logger.LogWarning("Performance review {ReviewId} not found for acknowledgment.", command.ReviewId);
            return (null, "Performance review not found");
        }

        if (review.EmployeeId != command.AcknowledgedBy)
        {
            _logger.LogWarning("User {UserId} attempted to acknowledge review {ReviewId} belonging to {EmployeeId}", 
                command.AcknowledgedBy, command.ReviewId, review.EmployeeId);
            return (null, "Only the employee being reviewed can acknowledge it");
        }

        if (review.Status != ReviewStatus.Submitted)
        {
            _logger.LogWarning("Review {ReviewId} is in status {Status}, but must be Submitted for acknowledgment.", 
                command.ReviewId, review.Status);
            return (null, "Review must be submitted by the manager before it can be acknowledged");
        }

        review.Status = ReviewStatus.Acknowledged;
        review.AcknowledgedDate = DateTime.UtcNow;
        review.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(review);

        await _publishEndpoint.Publish(new PerformanceReviewSubmittedEvent(
            review.Id,
            review.EmployeeId,
            review.OverallRating,
            review.AcknowledgedDate.Value));

        _logger.LogInformation("Performance review {ReviewId} acknowledged by employee {EmployeeId}", 
            review.Id, review.EmployeeId);

        return (review, null);
    }
}
