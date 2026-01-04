using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class UpdateGoalProgressCommandHandlerTests
{
    private readonly Mock<IGoalRepository> _repositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<INotificationServiceClient> _notificationServiceMock = new();
    private readonly UpdateGoalProgressValidator _validator = new();
    private readonly Mock<ILogger<UpdateGoalProgressCommandHandler>> _loggerMock = new();
    private readonly UpdateGoalProgressCommandHandler _handler;

    public UpdateGoalProgressCommandHandlerTests()
    {
        _handler = new UpdateGoalProgressCommandHandler(
            _repositoryMock.Object,
            _publishEndpointMock.Object,
            _notificationServiceMock.Object,
            _validator,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidProgressUpdate_UpdatesGoal()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = Guid.NewGuid(),
            CurrentStatus = GoalStatus.NotStarted,
            Description = "Test Goal"
        };

        var command = new UpdateGoalProgressCommand(
            goalId,
            "I started working on it",
            GoalStatus.InProgress,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var (updatedGoal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(updatedGoal);
        Assert.Equal(GoalStatus.InProgress, updatedGoal.CurrentStatus);
        Assert.Contains("I started working on it", updatedGoal.ProgressUpdates!);
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MarkAsCompleted_PublishesEvent()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = Guid.NewGuid(),
            CurrentStatus = GoalStatus.InProgress,
            Description = "Test Goal"
        };

        var command = new UpdateGoalProgressCommand(
            goalId,
            "Finished!",
            GoalStatus.Completed,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var (updatedGoal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.Equal(GoalStatus.Completed, updatedGoal!.CurrentStatus);
        Assert.NotNull(updatedGoal.CompletionDate);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PerformanceGoalCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MarkAsAtRisk_SendsAlert()
    {
        // Arrange
        var goalId = Guid.NewGuid();
        var goal = new Goal
        {
            Id = goalId,
            EmployeeId = Guid.NewGuid(),
            CurrentStatus = GoalStatus.InProgress,
            Description = "Test Goal"
        };

        var command = new UpdateGoalProgressCommand(
            goalId,
            "Having issues",
            GoalStatus.AtRisk,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.GetByIdAsync(goalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(goal);

        // Act
        var (updatedGoal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.Equal(GoalStatus.AtRisk, updatedGoal!.CurrentStatus);
        _notificationServiceMock.Verify(x => x.SendGoalAtRiskAlertAsync(goal.EmployeeId, goal.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
