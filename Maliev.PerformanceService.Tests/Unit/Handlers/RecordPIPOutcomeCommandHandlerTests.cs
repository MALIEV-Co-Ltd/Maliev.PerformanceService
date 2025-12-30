using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class RecordPIPOutcomeCommandHandlerTests
{
    private readonly Mock<IPIPRepository> _pipRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<ILogger<RecordPIPOutcomeCommandHandler>> _loggerMock = new();
    private readonly RecordPIPOutcomeCommandHandler _handler;

    public RecordPIPOutcomeCommandHandlerTests()
    {
        _handler = new RecordPIPOutcomeCommandHandler(
            _pipRepositoryMock.Object,
            _publishEndpointMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulOutcome_CompletesPIP()
    {
        // Arrange
        var pipId = Guid.NewGuid();
        var pip = new PerformanceImprovementPlan { Id = pipId, Status = PIPStatus.Active, EndDate = DateTime.UtcNow };
        var command = new RecordPIPOutcomeCommand(pipId, PIPOutcome.Successful, null, Guid.NewGuid());

        _pipRepositoryMock.Setup(x => x.GetByIdAsync(pipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pip);

        // Act
        var (updatedPip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.Equal(PIPStatus.Completed, updatedPip!.Status);
        Assert.Equal(PIPOutcome.Successful, updatedPip.Outcome);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<PIPCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_FirstExtension_ExtendsPIP()
    {
        // Arrange
        var pipId = Guid.NewGuid();
        var endDate = DateTime.UtcNow.AddMonths(1);
        var pip = new PerformanceImprovementPlan { Id = pipId, Status = PIPStatus.Active, EndDate = endDate, ExtensionCount = 0 };
        var newEndDate = endDate.AddMonths(1);
        var command = new RecordPIPOutcomeCommand(pipId, PIPOutcome.ExtendedAgain, newEndDate, Guid.NewGuid());

        _pipRepositoryMock.Setup(x => x.GetByIdAsync(pipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pip);

        // Act
        var (updatedPip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.Equal(PIPStatus.Extended, updatedPip!.Status);
        Assert.Equal(1, updatedPip.ExtensionCount);
        Assert.Equal(newEndDate, updatedPip.EndDate);
    }

    [Fact]
    public async Task HandleAsync_SecondExtension_ReturnsError()
    {
        // Arrange
        var pipId = Guid.NewGuid();
        var pip = new PerformanceImprovementPlan { Id = pipId, Status = PIPStatus.Extended, ExtensionCount = 1 };
        var command = new RecordPIPOutcomeCommand(pipId, PIPOutcome.ExtendedAgain, DateTime.UtcNow.AddMonths(1), Guid.NewGuid());

        _pipRepositoryMock.Setup(x => x.GetByIdAsync(pipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pip);

        // Act
        var (updatedPip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.NotNull(error);
        Assert.Equal("Maximum of one extension is allowed for PIP.", error);
        Assert.Null(updatedPip);
    }
}
