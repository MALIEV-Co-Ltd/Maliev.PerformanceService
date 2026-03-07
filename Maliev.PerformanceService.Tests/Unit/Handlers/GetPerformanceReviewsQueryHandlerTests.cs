using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class GetPerformanceReviewsQueryHandlerTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock = new();
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly GetPerformanceReviewsQueryHandler _handler;

    public GetPerformanceReviewsQueryHandlerTests()
    {
        _handler = new GetPerformanceReviewsQueryHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_UserRequestingOwnReviews_ReturnsReviews()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var userId = employeeId;
        var reviews = new List<PerformanceReview>
        {
            new PerformanceReview { Id = Guid.NewGuid(), EmployeeId = employeeId, ReviewCycle = ReviewCycle.Annual },
            new PerformanceReview { Id = Guid.NewGuid(), EmployeeId = employeeId, ReviewCycle = ReviewCycle.SemiAnnual }
        };

        var query = new GetPerformanceReviewsQuery(employeeId, userId);

        _repositoryMock.Setup(x => x.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task HandleAsync_ManagerAccessingReportsReviews_ReturnsReviews()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var reviews = new List<PerformanceReview>
        {
            new PerformanceReview { Id = Guid.NewGuid(), EmployeeId = employeeId }
        };

        var query = new GetPerformanceReviewsQuery(employeeId, managerId);

        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(managerId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(x => x.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task HandleAsync_NonManagerAccessingOtherUsersReviews_ReturnsEmpty()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var query = new GetPerformanceReviewsQuery(employeeId, otherUserId);

        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(otherUserId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
        _repositoryMock.Verify(x => x.GetByEmployeeIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoReviewsFound_ReturnsEmpty()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var userId = employeeId;

        var query = new GetPerformanceReviewsQuery(employeeId, userId);

        _repositoryMock.Setup(x => x.GetByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<PerformanceReview>());

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(result);
    }
}
