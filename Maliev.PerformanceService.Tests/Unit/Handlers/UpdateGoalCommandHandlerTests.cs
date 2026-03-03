using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class UpdateGoalCommandHandlerTests
{
    private readonly Mock<IGoalRepository> _repositoryMock = new();
    private readonly Mock<ILogger<UpdateGoalCommandHandler>> _loggerMock = new();
    private readonly UpdateGoalCommandHandler _handler;

    public UpdateGoalCommandHandlerTests()
    {
        _handler = new UpdateGoalCommandHandler(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_UpdatesGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var existingGoal = new Goal
        {
            Id = goalId,
            EmployeeId = Guid.NewGuid(),
            Description = "Old Description",
            SuccessCriteria = "Old Criteria",
            CurrentStatus = GoalStatus.InProgress,
            CreatedDate = DateTime.UtcNow.AddDays(-7),
            ModifiedDate = DateTime.UtcNow.AddDays(-7)
        };

        var command = new UpdateGoalCommand(
            goalId,
            "New Description",
            "New Success Criteria",
            DateTime.UtcNow.AddMonths(1),
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGoal);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal g, CancellationToken _) => g);

        // Act
        var (goal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(goal);
        Assert.Equal("New Description", goal.Description);
        Assert.Equal("New Success Criteria", goal.SuccessCriteria);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_GoalNotFound_ReturnsError()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var command = new UpdateGoalCommand(
            goalId,
            "New Description",
            "New Success Criteria",
            DateTime.UtcNow.AddMonths(1),
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal?)null);

        // Act
        var (goal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Goal not found.", error);
        Assert.Null(goal);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_UpdatesModifiedDate()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var existingGoal = new Goal
        {
            Id = goalId,
            EmployeeId = Guid.NewGuid(),
            Description = "Old Description",
            SuccessCriteria = "Old Criteria",
            CurrentStatus = GoalStatus.InProgress,
            CreatedDate = DateTime.UtcNow.AddDays(-7),
            ModifiedDate = DateTime.UtcNow.AddDays(-7)
        };

        var command = new UpdateGoalCommand(
            goalId,
            "New Description",
            "New Success Criteria",
            DateTime.UtcNow.AddMonths(1),
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingGoal);
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal g, CancellationToken _) => g);

        var beforeHandle = DateTime.UtcNow;

        // Act
        var (goal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(goal);
        Assert.True(goal.ModifiedDate >= beforeHandle);
    }
}
