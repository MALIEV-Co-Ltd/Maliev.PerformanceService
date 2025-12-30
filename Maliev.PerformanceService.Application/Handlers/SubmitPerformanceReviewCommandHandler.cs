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

        // TODO: Authorization check - requesting user must be the manager of the employee
        
        review.ManagerAssessment = command.ManagerAssessment;
        review.OverallRating = command.OverallRating;
        review.Status = ReviewStatus.Submitted;
        review.ModifiedDate = DateTime.UtcNow;

        await _repository.UpdateAsync(review, cancellationToken);

        // TODO: Notify employee
        
        return (review, null);
    }
}