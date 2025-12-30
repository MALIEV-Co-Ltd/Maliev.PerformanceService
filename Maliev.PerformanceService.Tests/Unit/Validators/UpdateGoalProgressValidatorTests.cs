using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Enums;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Validators;

public class UpdateGoalProgressValidatorTests
{
    private readonly UpdateGoalProgressValidator _validator = new();

    [Fact]
    public void Validate_ValidTransition_ReturnsTrue()
    {
        // Arrange
        var command = new UpdateGoalProgressCommand(
            Guid.NewGuid(),
            "Working on it",
            GoalStatus.InProgress,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command, GoalStatus.NotStarted);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyUpdate_ReturnsFalse()
    {
        // Arrange
        var command = new UpdateGoalProgressCommand(
            Guid.NewGuid(),
            "",
            GoalStatus.InProgress,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command, GoalStatus.NotStarted);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Progress update text is required.", result.Error);
    }

    [Fact]
    public void Validate_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var command = new UpdateGoalProgressCommand(
            Guid.NewGuid(),
            "Completed",
            GoalStatus.Completed,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command, GoalStatus.NotStarted);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid status transition", result.Error!);
    }
}
