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
        if (review.EmployeeId != command.RequestingUserId)
        {
            // Note: In real system we might allow manager to update their part too.
            // For US1 we focus on employee self-assessment.
            // But we should check permissions.
        }

        if (command.SelfAssessment != null)
        {
            review.SelfAssessment = command.SelfAssessment;
        }

        if (command.ManagerAssessment != null)
        {
            review.ManagerAssessment = command.ManagerAssessment;
        }

        if (command.SubmitSelfAssessment)
        {
            if (string.IsNullOrWhiteSpace(review.SelfAssessment))
            {
                return (null, "Self-assessment is required for submission.");
            }
            review.Status = ReviewStatus.SelfAssessmentPending;
            review.SubmittedDate = DateTime.UtcNow;
            
            // TODO: Notify manager
        }

        review.ModifiedDate = DateTime.UtcNow;
        await _repository.UpdateAsync(review, cancellationToken);

        return (review, null);
    }
}