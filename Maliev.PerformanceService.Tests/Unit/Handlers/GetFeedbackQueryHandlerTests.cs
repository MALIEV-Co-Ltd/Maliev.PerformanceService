using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class GetFeedbackQueryHandlerTests
{
    private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock = new();
    private readonly Mock<IPerformanceReviewRepository> _reviewRepositoryMock = new();
    private readonly Mock<ILogger<GetFeedbackQueryHandler>> _loggerMock = new();
    private readonly GetFeedbackQueryHandler _handler;

    public GetFeedbackQueryHandlerTests()
    {
        _handler = new GetFeedbackQueryHandler(
            _feedbackRepositoryMock.Object,
            _reviewRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_AuthorizedUser_ReturnsFeedback()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, EmployeeId = employeeId };
        
        var feedbackItems = new List<ReviewFeedback>
        {
            new ReviewFeedback { FeedbackType = FeedbackType.Peer, Feedback = "F1", IsAnonymous = false, ProviderId = Guid.NewGuid() },
            new ReviewFeedback { FeedbackType = FeedbackType.Peer, Feedback = "F2", IsAnonymous = false, ProviderId = Guid.NewGuid() }
        };

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _feedbackRepositoryMock.Setup(x => x.GetByReviewIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedbackItems);

        var query = new GetFeedbackQuery(reviewId, employeeId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task HandleAsync_SingleAnonymousFeedback_SuppressesIt()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, EmployeeId = employeeId };
        
        var feedbackItems = new List<ReviewFeedback>
        {
            new ReviewFeedback { FeedbackType = FeedbackType.Peer, Feedback = "Secret", IsAnonymous = true, ProviderId = Guid.NewGuid() }
        };

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _feedbackRepositoryMock.Setup(x => x.GetByReviewIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedbackItems);

        var query = new GetFeedbackQuery(reviewId, employeeId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_MultipleAnonymousFeedback_ReturnsMasked()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, EmployeeId = employeeId };
        
        var feedbackItems = new List<ReviewFeedback>
        {
            new ReviewFeedback { FeedbackType = FeedbackType.Peer, Feedback = "Secret 1", IsAnonymous = true, ProviderId = Guid.NewGuid() },
            new ReviewFeedback { FeedbackType = FeedbackType.Peer, Feedback = "Secret 2", IsAnonymous = true, ProviderId = Guid.NewGuid() }
        };

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);
        _feedbackRepositoryMock.Setup(x => x.GetByReviewIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feedbackItems);

        var query = new GetFeedbackQuery(reviewId, employeeId);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, f => Assert.Equal(Guid.Empty, f.ProviderId));
    }
}
