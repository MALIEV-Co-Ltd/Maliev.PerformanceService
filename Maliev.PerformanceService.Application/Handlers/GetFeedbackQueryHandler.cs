using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles retrieving and aggregating feedback for a review.
/// </summary>
public class GetFeedbackQueryHandler
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IPerformanceReviewRepository _reviewRepository;
    private readonly ILogger<GetFeedbackQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFeedbackQueryHandler"/> class.
    /// </summary>
    public GetFeedbackQueryHandler(
        IFeedbackRepository feedbackRepository,
        IPerformanceReviewRepository reviewRepository,
        ILogger<GetFeedbackQueryHandler> logger)
    {
        _feedbackRepository = feedbackRepository;
        _reviewRepository = reviewRepository;
        _logger = logger;
    }

    /// <summary>
    /// Executes the retrieval of aggregated feedback.
    /// </summary>
    /// <param name="query">The query details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of feedback items.</returns>
    public async Task<IEnumerable<ReviewFeedback>> HandleAsync(GetFeedbackQuery query, CancellationToken cancellationToken = default)
    {
        var review = await _reviewRepository.GetByIdAsync(query.PerformanceReviewId, cancellationToken);
        if (review == null)
        {
            return Enumerable.Empty<ReviewFeedback>();
        }

        // Authorization check: Only employee being reviewed, manager, or HR can view feedback
        // In a real system, we'd check against a more robust permission set.
        // For now, we allow the requesting user if they are the employee or the reviewer.
        if (review.EmployeeId != query.RequestingUserId && review.ReviewerId != query.RequestingUserId)
        {
            _logger.LogWarning("User {UserId} not authorized to view feedback for review {ReviewId}.", 
                query.RequestingUserId, query.PerformanceReviewId);
            return Enumerable.Empty<ReviewFeedback>();
        }

        var allFeedback = await _feedbackRepository.GetByReviewIdAsync(query.PerformanceReviewId, cancellationToken);
        
        var feedbackList = allFeedback.ToList();
        var result = new List<ReviewFeedback>();

        var groups = feedbackList.GroupBy(f => f.FeedbackType);

        foreach (var group in groups)
        {
            var groupItems = group.ToList();
            
            // If only one provider of this type and it's anonymous, suppress it to protect identity
            if (groupItems.Count == 1 && groupItems[0].IsAnonymous)
            {
                _logger.LogInformation("Suppressing anonymous feedback for review {ReviewId} as only one provider exists for type {FeedbackType}.", 
                    query.PerformanceReviewId, group.Key);
                continue;
            }

            foreach (var item in groupItems)
            {
                if (item.IsAnonymous)
                {
                    // Mask ProviderId in the result for anonymous feedback
                    item.ProviderId = Guid.Empty;
                }
                result.Add(item);
            }
        }

        return result;
    }
}
