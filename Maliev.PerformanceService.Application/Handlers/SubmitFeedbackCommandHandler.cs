using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the submission of 360-degree feedback for a review.
/// </summary>
public class SubmitFeedbackCommandHandler
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly INotificationServiceClient _notificationService;
    private readonly SubmitFeedbackValidator _validator;
    private readonly ILogger<SubmitFeedbackCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubmitFeedbackCommandHandler"/> class.
    /// </summary>
    public SubmitFeedbackCommandHandler(
        IFeedbackRepository feedbackRepository,
        IPerformanceReviewRepository reviewRepository,
        INotificationServiceClient notificationService,
        SubmitFeedbackValidator validator,
        ILogger<SubmitFeedbackCommandHandler> logger)
    {
        _feedbackRepository = feedbackRepository;
        _reviewRepository = reviewRepository;
        _notificationService = notificationService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Executes the submission of feedback.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created feedback or an error message.</returns>
    public async Task<(ReviewFeedback? Feedback, string? Error)> HandleAsync(SubmitFeedbackCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = _validator.Validate(command);
        if (!validationResult.IsValid)
        {
            return (null, validationResult.Error);
        }

        var review = await _reviewRepository.GetByIdAsync(command.PerformanceReviewId, cancellationToken);
        if (review == null)
        {
            return (null, "Performance review not found.");
        }

        // Data Volume Limits
        var count = await _feedbackRepository.CountByEmployeeIdAsync(review.EmployeeId, cancellationToken);
        if (count >= 200)
        {
            return (null, "DATA_VOLUME_LIMIT_REACHED: Maximum of 200 feedback entries per employee.");
        }
        if (count >= 160)
        {
            await _notificationService.SendDataVolumeWarningAsync(review.EmployeeId, "ReviewFeedback", count, 200, cancellationToken);
        }

        var feedback = new ReviewFeedback
        {
            Id = Guid.NewGuid(),
            PerformanceReviewId = command.PerformanceReviewId,
            ProviderId = command.ProviderId,
            FeedbackType = command.FeedbackType,
            Feedback = command.Feedback,
            IsAnonymous = command.IsAnonymous,
            SubmittedDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        var typeCount = await _feedbackRepository.GetFeedbackCountByTypeAsync(command.PerformanceReviewId, (int)command.FeedbackType, cancellationToken);
        if (command.IsAnonymous && typeCount == 0)
        {
            _logger.LogInformation("Anonymity warning: Only one provider of type {FeedbackType} for review {ReviewId}. Anonymity may be compromised.", 
                command.FeedbackType, command.PerformanceReviewId);
        }

        var createdFeedback = await _feedbackRepository.CreateAsync(feedback, cancellationToken);
        _logger.LogInformation("Feedback {FeedbackId} submitted for review {ReviewId} by user {UserId}.", 
            createdFeedback.Id, command.PerformanceReviewId, command.ProviderId);

        return (createdFeedback, null);
    }
}
