using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class GetGoalsQueryHandlerTests
{
    private readonly Mock<IGoalRepository> _repositoryMock = new();
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly GetGoalsQueryHandler _handler;

    public GetGoalsQueryHandlerTests()
    {
        _handler = new GetGoalsQueryHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_UserRequestingOwnGoals_ReturnsGoals()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var userId = employeeId;
        var goals = new List<Goal>
        {
            new Goal { Id = Guid.NewGuid(), EmployeeId = employeeId, Description = "Goal 1" },
            new Goal { Id = Guid.NewGuid(), EmployeeId = employeeId, Description = "Goal 2" }
        };

        var query = new GetGoalsQuery(employeeId, null, 10, userId);

        _repositoryMock.Setup(x => x.GetByEmployeeIdPaginatedAsync(employeeId, null, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((goals, (Guid?)Guid.NewGuid()));

        // Act
        var (items, cursor) = await _handler.HandleAsync(query);

        // Assert
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task HandleAsync_ManagerAccessingReportsGoals_ReturnsGoals()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var goals = new List<Goal>
        {
            new Goal { Id = Guid.NewGuid(), EmployeeId = employeeId, Description = "Goal 1" }
        };

        var query = new GetGoalsQuery(employeeId, null, 10, managerId);

        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(managerId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(x => x.GetByEmployeeIdPaginatedAsync(employeeId, null, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((goals, (Guid?)null));

        // Act
        var (items, cursor) = await _handler.HandleAsync(query);

        // Assert
        Assert.Single(items);
    }

    [Fact]
    public async Task HandleAsync_NonManagerAccessingOtherUsersGoals_ReturnsEmpty()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var query = new GetGoalsQuery(employeeId, null, 10, otherUserId);

        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(otherUserId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var (items, cursor) = await _handler.HandleAsync(query);

        // Assert
        Assert.Empty(items);
        Assert.Null(cursor);
        _repositoryMock.Verify(x => x.GetByEmployeeIdPaginatedAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithCursor_PassesCursorToRepository()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var userId = employeeId;
        var cursor = Guid.NewGuid();

        var query = new GetGoalsQuery(employeeId, cursor, 10, userId);

        _repositoryMock.Setup(x => x.GetByEmployeeIdPaginatedAsync(employeeId, cursor, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Enumerable.Empty<Goal>(), (Guid?)null));

        // Act
        await _handler.HandleAsync(query);

        // Assert
        _repositoryMock.Verify(x => x.GetByEmployeeIdPaginatedAsync(employeeId, cursor, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
