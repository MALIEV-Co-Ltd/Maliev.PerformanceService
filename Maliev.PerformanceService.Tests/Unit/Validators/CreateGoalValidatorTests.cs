using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Validators;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Validators;

public class CreateGoalValidatorTests
{
    private readonly CreateGoalValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ReturnsTrue()
    {
        // Arrange
        var command = new CreateGoalCommand(
            Guid.NewGuid(),
            "Finish project",
            "Project is deployed",
            DateTime.UtcNow.AddDays(7),
            null,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyDescription_ReturnsFalse()
    {
        // Arrange
        var command = new CreateGoalCommand(
            Guid.NewGuid(),
            "",
            "Project is deployed",
            DateTime.UtcNow.AddDays(7),
            null,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Goal description is required.", result.Error);
    }

    [Fact]
    public void Validate_PastTargetDate_ReturnsFalse()
    {
        // Arrange
        var command = new CreateGoalCommand(
            Guid.NewGuid(),
            "Finish project",
            "Project is deployed",
            DateTime.UtcNow.AddDays(-1),
            null,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Target completion date must be in the future.", result.Error);
    }
}
