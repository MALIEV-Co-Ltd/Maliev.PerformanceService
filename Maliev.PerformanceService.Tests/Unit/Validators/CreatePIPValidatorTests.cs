using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Validators;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Validators;

public class CreatePIPValidatorTests
{
    private readonly CreatePIPValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ReturnsValid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "Performance issues",
            "Communication, Time management",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_StartDateEqualsEndDate_ReturnsInvalid()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(30);
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            date,
            date,
            "Performance issues",
            "Communication, Time management",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Start date must be before end date.", error);
    }

    [Fact]
    public void Validate_StartDateAfterEndDate_ReturnsInvalid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(-30),
            "Performance issues",
            "Communication, Time management",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Start date must be before end date.", error);
    }

    [Fact]
    public void Validate_EmptyReason_ReturnsInvalid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "",
            "Communication, Time management",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Reason is required.", error);
    }

    [Fact]
    public void Validate_WhitespaceReason_ReturnsInvalid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "   ",
            "Communication, Time management",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Reason is required.", error);
    }

    [Fact]
    public void Validate_EmptyImprovementAreas_ReturnsInvalid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "Performance issues",
            "",
            "Meet deadlines consistently",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Improvement areas are required.", error);
    }

    [Fact]
    public void Validate_EmptySuccessCriteria_ReturnsInvalid()
    {
        // Arrange
        var command = new CreatePIPCommand(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "Performance issues",
            "Communication, Time management",
            "",
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Success criteria are required.", error);
    }
}
