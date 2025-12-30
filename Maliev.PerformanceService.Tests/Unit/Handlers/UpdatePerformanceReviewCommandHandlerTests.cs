using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class UpdatePerformanceReviewCommandHandlerTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock;
    private readonly Mock<INotificationServiceClient> _notificationServiceMock;
    private readonly Mock<ILogger<UpdatePerformanceReviewCommandHandler>> _loggerMock;
    private readonly UpdatePerformanceReviewCommandHandler _handler;

    public UpdatePerformanceReviewCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPerformanceReviewRepository>();
        _notificationServiceMock = new Mock<INotificationServiceClient>();
        _loggerMock = new Mock<ILogger<UpdatePerformanceReviewCommandHandler>>();
        _handler = new UpdatePerformanceReviewCommandHandler(
            _repositoryMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ReviewNotFound_ReturnsError()
    {
        // Arrange
        var command = new UpdatePerformanceReviewCommand(Guid.NewGuid(), "Self", null, false, Guid.NewGuid());
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default)).ReturnsAsync((PerformanceReview?)null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Review);
        Assert.Equal("Performance review not found.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_SubmitWithoutAssessment_ReturnsError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, EmployeeId = employeeId, Status = ReviewStatus.Draft };
        var command = new UpdatePerformanceReviewCommand(reviewId, "", null, true, employeeId);
        
        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, default)).ReturnsAsync(review);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Review);
        Assert.Equal("Self-assessment is required for submission.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_ValidSubmit_UpdatesStatus()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview { Id = reviewId, EmployeeId = employeeId, Status = ReviewStatus.Draft, SelfAssessment = "Existing" };
        var command = new UpdatePerformanceReviewCommand(reviewId, "Updated", null, true, employeeId);
        
        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, default)).ReturnsAsync(review);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<PerformanceReview>(), default)).ReturnsAsync((PerformanceReview r, CancellationToken ct) => r);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Review);
        Assert.Equal("Updated", result.Review.SelfAssessment);
        Assert.Equal(ReviewStatus.SelfAssessmentPending, result.Review.Status);
        Assert.NotNull(result.Review.SubmittedDate);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<PerformanceReview>(), default), Times.Once);
    }
}
