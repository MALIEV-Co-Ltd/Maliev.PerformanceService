using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles updates to a performance review, such as saving self-assessment drafts.
/// </summary>
public class UpdatePerformanceReviewCommandHandler
{
    private readonly IPerformanceReviewRepository _repository;
    private readonly INotificationServiceClient _notificationService;
    private readonly ILogger<UpdatePerformanceReviewCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePerformanceReviewCommandHandler"/> class.
    /// </summary>
    public UpdatePerformanceReviewCommandHandler(
        IPerformanceReviewRepository repository,
        INotificationServiceClient notificationService,
        ILogger<UpdatePerformanceReviewCommandHandler> logger)
    {
        _repository = repository;
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Executes an update to a performance review record.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated review or an error message.</returns>
    public async Task<(PerformanceReview? Review, string? Error)> HandleAsync(UpdatePerformanceReviewCommand command, CancellationToken cancellationToken = default)
    {
        var review = await _repository.GetByIdAsync(command.ReviewId, cancellationToken);
        if (review == null)
        {
            return (null, "Performance review not found.");
        }

        // Basic authorization: only employee can update their own self-assessment
        // Note: Managers can update manager assessment, but that's handled in SubmitPerformanceReviewCommandHandler
        // However, we should allow draft saving for both.
        if (review.EmployeeId != command.RequestingUserId && review.ReviewerId != command.RequestingUserId)
        {
            _logger.LogWarning("Unauthorized attempt to update review {ReviewId} by user {UserId}.", command.ReviewId, command.RequestingUserId);
            return (null, "You are not authorized to update this review.");
        }

        if (command.SelfAssessment != null)
        {
            if (review.EmployeeId != command.RequestingUserId)
            {
                return (null, "Only the employee can update the self-assessment.");
            }
            review.SelfAssessment = command.SelfAssessment;
        }

        if (command.ManagerAssessment != null)
        {
            if (review.ReviewerId != command.RequestingUserId)
            {
                return (null, "Only the assigned reviewer can update the manager assessment.");
            }
            review.ManagerAssessment = command.ManagerAssessment;
        }

        if (command.SubmitSelfAssessment)
        {
            if (review.EmployeeId != command.RequestingUserId)
            {
                return (null, "Only the employee can submit the self-assessment.");
            }

            if (string.IsNullOrWhiteSpace(review.SelfAssessment))
            {
                return (null, "Self-assessment is required for submission.");
            }
            review.Status = ReviewStatus.SelfAssessmentPending;
            review.SubmittedDate = DateTime.UtcNow;

            // Notify manager
            try
            {
                await _notificationService.SendReviewReminderAsync(review.ReviewerId, review.Id, "ManagerReview", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to manager {ReviewerId} for review {ReviewId}", review.ReviewerId, review.Id);
            }
        }

        review.ModifiedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(review, cancellationToken);

        return (review, null);
    }
}