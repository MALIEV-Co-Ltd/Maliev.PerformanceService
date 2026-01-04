using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class AcknowledgePerformanceReviewCommandHandlerTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<AcknowledgePerformanceReviewCommandHandler>> _loggerMock = new();
    private readonly AcknowledgePerformanceReviewCommandHandler _handler;

    public AcknowledgePerformanceReviewCommandHandlerTests()
    {
        _handler = new AcknowledgePerformanceReviewCommandHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidAcknowledgment_UpdatesStatusAndPublishesEvent()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview
        {
            Id = reviewId,
            EmployeeId = employeeId,
            Status = ReviewStatus.Submitted,
            OverallRating = PerformanceRating.MeetsExpectations
        };

        var command = new AcknowledgePerformanceReviewCommand(reviewId, employeeId);

        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var (updatedReview, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(updatedReview);
        Assert.Equal(ReviewStatus.Acknowledged, updatedReview.Status);
        Assert.NotNull(updatedReview.AcknowledgedDate);
        _repositoryMock.Verify(x => x.UpdateAsync(review, It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PerformanceReviewSubmittedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WrongEmployee_ReturnsError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview
        {
            Id = reviewId,
            EmployeeId = employeeId,
            Status = ReviewStatus.Submitted
        };

        var command = new AcknowledgePerformanceReviewCommand(reviewId, Guid.NewGuid()); // Different employee

        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var (updatedReview, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Only the employee being reviewed can acknowledge it", error);
        Assert.Null(updatedReview);
    }

    [Fact]
    public async Task HandleAsync_WrongStatus_ReturnsError()
    {
        // Arrange
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var review = new PerformanceReview
        {
            Id = reviewId,
            EmployeeId = employeeId,
            Status = ReviewStatus.Draft // Not Submitted
        };

        var command = new AcknowledgePerformanceReviewCommand(reviewId, employeeId);

        _repositoryMock.Setup(x => x.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        // Act
        var (updatedReview, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Contains("must be submitted by the manager", error);
        Assert.Null(updatedReview);
    }
}
