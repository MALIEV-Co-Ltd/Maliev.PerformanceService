using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using MassTransit;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class CreatePerformanceReviewCommandHandlerTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock;
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock;
    private readonly Mock<INotificationServiceClient> _notificationServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreatePerformanceReviewValidator _validator;
    private readonly CreatePerformanceReviewCommandHandler _handler;

    public CreatePerformanceReviewCommandHandlerTests()
    {
        _repositoryMock = new Mock<IPerformanceReviewRepository>();
        _employeeServiceMock = new Mock<IEmployeeServiceClient>();
        _notificationServiceMock = new Mock<INotificationServiceClient>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _validator = new CreatePerformanceReviewValidator(_repositoryMock.Object);
        _handler = new CreatePerformanceReviewCommandHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object,
            _notificationServiceMock.Object,
            _validator,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task HandleAsync_EmployeeNotFound_ReturnsError()
    {
        // Arrange
        var command = new CreatePerformanceReviewCommand(
            Guid.NewGuid(),
            ReviewCycle.Annual,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            null,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.ExistsOverlappingReviewAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, default))
            .ReturnsAsync(false);
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(result.Review);
        Assert.Equal("Employee not found.", result.Error);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesReview()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreatePerformanceReviewCommand(
            employeeId,
            ReviewCycle.Annual,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            "My self assessment",
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.ExistsOverlappingReviewAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, default))
            .ReturnsAsync(false);
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, default))
            .ReturnsAsync(true);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<PerformanceReview>(), default))
            .ReturnsAsync((PerformanceReview r, CancellationToken ct) => r);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(result.Review);
        Assert.Equal(employeeId, result.Review.EmployeeId);
        Assert.Equal(ReviewStatus.Draft, result.Review.Status);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<PerformanceReview>(), default), Times.Once);
    }
}
