using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class SubmitPerformanceReviewCommandHandlerTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock;
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock;
    private readonly Mock<INotificationServiceClient> _notificationServiceMock;
    private readonly Mock<ILogger<SubmitPerformanceReviewCommandHandler>> _loggerMock;
    private readonly SubmitPerformanceReviewCommandHandler _handler;

    public SubmitPerformanceReviewCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPerformanceReviewRepository>();
        _employeeServiceMock = new Mock<IEmployeeServiceClient>();
        _notificationServiceMock = new Mock<INotificationServiceClient>();
        _loggerMock = new Mock<ILogger<SubmitPerformanceReviewCommandHandler>>();
        _handler = new SubmitPerformanceReviewCommandHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReviewNotFound_ReturnsError()
    {
        // Arrange
        var command = new SubmitPerformanceReviewCommand(Guid.NewGuid(), "Great", PerformanceRating.ExceedsExpectations, Guid.NewGuid());
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((PerformanceReview?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Review);
        Assert.Equal("Performance review not found.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_InvalidStatus_ReturnsError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, Status = ReviewStatus.Draft };
        var command = new SubmitPerformanceReviewCommand(reviewId, "Great", PerformanceRating.ExceedsExpectations, Guid.NewGuid());
        
        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, default)).ReturnsAsync(review);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Review);
        Assert.Equal("Review must have a completed self-assessment before manager submission.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_ValidSubmit_UpdatesStatus()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, Status = ReviewStatus.SelfAssessmentPending };
        var command = new SubmitPerformanceReviewCommand(reviewId, "Great job", PerformanceRating.ExceedsExpectations, Guid.NewGuid());
        
        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, default)).ReturnsAsync(review);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PerformanceReview>(), default)).ReturnsAsync((PerformanceReview r, CancellationToken ct) => r);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Review);
        Assert.Equal("Great job", result.Review.ManagerAssessment);
        Assert.Equal(PerformanceRating.ExceedsExpectations, result.Review.OverallRating);
        Assert.Equal(ReviewStatus.Submitted, result.Review.Status);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PerformanceReview>(), default), Times.Once);
    }
}
