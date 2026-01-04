using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class CreatePIPCommandHandlerTests
{
    private readonly Mock<IPIPRepository> _pipRepositoryMock = new();
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly CreatePIPValidator _validator = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<CreatePIPCommandHandler>> _loggerMock = new();
    private readonly CreatePIPCommandHandler _handler;

    public CreatePIPCommandHandlerTests()
    {
        _handler = new CreatePIPCommandHandler(
            _pipRepositoryMock.Object,
            _employeeServiceMock.Object,
            _validator,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesPIP()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreatePIPCommand(
            employeeId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            "Poor performance",
            "Speed",
            "Finish tasks",
            Guid.NewGuid());

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _pipRepositoryMock.Setup(x => x.GetActivePIPByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerformanceImprovementPlan?)null);
        _pipRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<PerformanceImprovementPlan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PerformanceImprovementPlan p, CancellationToken _) => p);

        // Act
        var (pip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.NotNull(pip);
        _pipRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<PerformanceImprovementPlan>(), It.IsAny<CancellationToken>()), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PerformancePIPInitiatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_AlreadyHasActivePIP_ReturnsError()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var command = new CreatePIPCommand(
            employeeId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMonths(1),
            "Reason", "Areas", "Criteria", Guid.NewGuid());

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _pipRepositoryMock.Setup(x => x.GetActivePIPByEmployeeIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PerformanceImprovementPlan());

        // Act
        var (pip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Employee already has an active PIP.", error);
        Assert.Null(pip);
    }
}
