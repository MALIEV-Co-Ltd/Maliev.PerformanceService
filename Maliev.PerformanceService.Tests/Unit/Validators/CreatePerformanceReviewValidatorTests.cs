using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Enums;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Validators;

public class CreatePerformanceReviewValidatorTests
{
    private readonly Mock<IPerformanceReviewRepository> _repositoryMock;
    private readonly CreatePerformanceReviewValidator _validator;

    public CreatePerformanceReviewValidatorTests()
    {
        _repositoryMock = new Mock<IPerformanceReviewRepository>();
        _validator = new CreatePerformanceReviewValidator(_repositoryMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_InvalidPeriod_ReturnsError()
    {
        // Arrange
        var command = new CreatePerformanceReviewCommand(
            Guid.NewGuid(),
            ReviewCycle.Annual,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow, // Start > End
            null,
            Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("Review period start must be before end.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_OverlappingReview_ReturnsError()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreatePerformanceReviewCommand(
            employeeId,
            ReviewCycle.Annual,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            null,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.ExistsOverlappingReviewAsync(
            employeeId, (int)ReviewCycle.Annual, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, default))
            .ReturnsAsync(true);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal("An overlapping review period already exists for this employee.", result.Error);
    }

    [Fact]
    public async Task ValidateAsync_ValidCommand_ReturnsSuccess()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreatePerformanceReviewCommand(
            employeeId,
            ReviewCycle.Annual,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            null,
            Guid.NewGuid());

        _repositoryMock.Setup(x => x.ExistsOverlappingReviewAsync(
            employeeId, (int)ReviewCycle.Annual, It.IsAny<DateTime>(), It.IsAny<DateTime>(), null, default))
            .ReturnsAsync(false);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Error);
    }
}
