using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class CreateGoalCommandHandlerTests
{
    private readonly Mock<IGoalRepository> _repositoryMock = new();
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly Mock<INotificationServiceClient> _notificationServiceMock = new();
    private readonly CreateGoalValidator _validator = new();
    private readonly Mock<ILogger<CreateGoalCommandHandler>> _loggerMock = new();
    private readonly CreateGoalCommandHandler _handler;

    public CreateGoalCommandHandlerTests()
    {
        _handler = new CreateGoalCommandHandler(
            _repositoryMock.Object,
            _employeeServiceMock.Object,
            _notificationServiceMock.Object,
            _validator,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesGoal()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreateGoalCommand(
            employeeId,
            "Learn .NET 10",
            "Build amazing apps",
            DateTime.UtcNow.AddMonths(1),
            null,
            Guid.NewGuid());

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _repositoryMock.Setup(x => x.CreateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Goal g, CancellationToken _) => g);

        // Act
        var (goal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(goal);
        Assert.Equal(command.Description, goal.Description);
        _repositoryMock.Verify(x => x.CreateAsync(It.IsAny<Goal>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EmployeeNotFound_ReturnsError()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreateGoalCommand(
            employeeId,
            "Learn .NET 10",
            "Build amazing apps",
            DateTime.UtcNow.AddMonths(1),
            null,
            Guid.NewGuid());

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var (goal, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Employee not found.", error);
        Assert.Null(goal);
    }
}
