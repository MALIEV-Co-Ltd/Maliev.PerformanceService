using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Enums;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Validators;

public class SubmitFeedbackValidatorTests
{
    private readonly SubmitFeedbackValidator _validator = new();

    [Fact]
    public void Validate_ValidFeedback_ReturnsValid()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FeedbackType.Peer,
            "Great work on the project!",
            false,
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.True(isValid);
        Assert.Null(error);
    }

    [Fact]
    public void Validate_EmptyFeedback_ReturnsInvalid()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FeedbackType.Peer,
            "",
            false,
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Feedback text is required.", error);
    }

    [Fact]
    public void Validate_WhitespaceFeedback_ReturnsInvalid()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FeedbackType.Peer,
            "   ",
            false,
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Feedback text is required.", error);
    }

    [Fact]
    public void Validate_NullFeedback_ReturnsInvalid()
    {
        // Arrange
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            FeedbackType.Peer,
            null!,
            false,
            Guid.NewGuid());

        // Act
        var (isValid, error) = _validator.Validate(command);

        // Assert
        Assert.False(isValid);
        Assert.Equal("Feedback text is required.", error);
    }
}
