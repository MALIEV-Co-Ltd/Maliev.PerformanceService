using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class GetGoalByIdQueryHandlerTests
{
    private readonly Mock<IGoalRepository> _repositoryMock = new();
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly GetGoalByIdQueryHandler _handler;

    public GetGoalByIdQueryHandlerTests()
    {
        _handler = new GetGoalByIdQueryHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_GoalExistsAndOwnedByUser_ReturnsGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = userId,
            Description = "Test Goal",
            CurrentStatus = GoalStatus.InProgress
        };

        var query = new GetGoalByIdQuery(goalId, userId);

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
    }

    [Fact]
    public async Task HandleAsync_GoalNotFound_ReturnsNull()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var query = new GetGoalByIdQuery(goalId, userId);

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_ManagerAccessingReport_ReturnsGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var managerId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = employeeId,
            Description = "Test Goal",
            CurrentStatus = GoalStatus.InProgress
        };

        var query = new GetGoalByIdQuery(goalId, managerId);

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);
        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(managerId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(goalId, result.Id);
    }

    [Fact]
    public async Task HandleAsync_NonManagerAccessingOtherUsersGoal_ReturnsNull()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = employeeId,
            Description = "Test Goal",
            CurrentStatus = GoalStatus.InProgress
        };

        var query = new GetGoalByIdQuery(goalId, otherUserId);

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);
        _employeeServiceMock.Setup(x => x.ValidateManagerEmployeeRelationshipAsync(otherUserId, employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(query);

        // Assert
        Assert.Null(result);
    }
}
