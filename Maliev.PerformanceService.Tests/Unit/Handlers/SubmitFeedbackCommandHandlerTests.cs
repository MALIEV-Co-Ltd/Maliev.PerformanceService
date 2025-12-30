using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class SubmitFeedbackCommandHandlerTests
{
    private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock = new();
    private readonly Mock<IPerformanceReviewRepository> _reviewRepositoryMock = new();
    private readonly Mock<INotificationServiceClient> _notificationServiceMock = new();
    private readonly SubmitFeedbackValidator _validator = new();
    private readonly Mock<ILogger<SubmitFeedbackCommandHandler>> _loggerMock = new();
    private readonly SubmitFeedbackCommandHandler _handler;

    public SubmitFeedbackCommandHandlerTests()
    {
        _handler = new SubmitFeedbackCommandHandler(
            _feedbackRepositoryMock.Object,
            _reviewRepositoryMock.Object,
            _notificationServiceMock.Object,
            _validator,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesFeedback()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(
            reviewId,
            providerId,
            FeedbackType.Peer,
            "Good work",
            false,
            providerId);

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceReview { Id = reviewId });
        _feedbackRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<ReviewFeedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReviewFeedback f, CancellationToken _) => f);

        // Act
        var (feedback, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(feedback);
        Assert.Equal(command.Feedback, feedback.Feedback);
        _feedbackRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ReviewFeedback>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ReviewNotFound_ReturnsError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(
            reviewId,
            Guid.NewGuid(),
            FeedbackType.Peer,
            "Good",
            false,
            Guid.NewGuid());

        _reviewRepositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerformanceReview?)null);

        // Act
        var (feedback, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Performance review not found.", error);
        Assert.Null(feedback);
    }
}
