using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the manager submission of a performance review.
/// </summary>
public class SubmitPerformanceReviewCommandHandler
{
    private readonly IPerformanceReviewRepository _repository;
    private readonly IEmployeeServiceClient _employeeService;
    private readonly INotificationServiceClient _notificationService;
    private readonly ILogger<SubmitPerformanceReviewCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitPerformanceReviewCommandHandler"/> class.
    /// </summary>
    public SubmitPerformanceReviewCommandHandler(
        IPerformanceReviewRepository repository,
        IEmployeeServiceClient employeeService,
        INotificationServiceClient notificationService,
        ILogger<SubmitPerformanceReviewCommandHandler> logger)
    {
        _repository = repository;
        _employeeService = employeeService;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Executes the formal submission of a review by a manager.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated review or an error message.</returns>
    public async Task<(PerformanceReview? Review, string? Error)> HandleAsync(SubmitPerformanceReviewCommand command, CancellationToken cancellationToken = default)
    {
        var review = await _repository.GetByIdAsync(command.ReviewId, cancellationToken);
        if (review == null)
        {
            return (null, "Performance review not found.");
        }

        if (review.Status != ReviewStatus.SelfAssessmentPending)
        {
            return (null, "Review must have a completed self-assessment before manager submission.");
        }

        // Authorization check: Only the assigned reviewer (manager) can submit
        if (review.ReviewerId != command.RequestingUserId)
        {
            _logger.LogWarning("Unauthorized attempt to submit review {ReviewId} by user {UserId}. Expected reviewer {ReviewerId}.",
                command.ReviewId, command.RequestingUserId, review.ReviewerId);
            return (null, "You are not authorized to submit this review.");
        }

        review.ManagerAssessment = command.ManagerAssessment;
        review.OverallRating = command.OverallRating;
        review.Status = ReviewStatus.Submitted;
        review.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(review, cancellationToken);

        // Notify employee
        try
        {
            await _notificationService.SendAcknowledgmentReminderAsync(review.EmployeeId, review.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to employee {EmployeeId} for review {ReviewId}", review.EmployeeId, review.Id);
            // Non-blocking error
        }

        return (review, null);
    }
}