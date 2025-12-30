using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Handlers;

public class UpdatePIPCommandHandlerTests
{
    private readonly Mock<IPIPRepository> _pipRepositoryMock = new();
    private readonly Mock<ILogger<UpdatePIPCommandHandler>> _loggerMock = new();
    private readonly UpdatePIPCommandHandler _handler;

    public UpdatePIPCommandHandlerTests()
    {
        _handler = new UpdatePIPCommandHandler(_pipRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidNote_AppendsNote()
    {
        // Arrange
        var pipId = Guid.NewGuid();
        var pip = new PerformanceImprovementPlan { Id = pipId, CheckInNotes = "[2025-01-01] Old note" };
        var command = new UpdatePIPCommand(pipId, "New note", Guid.NewGuid());

        _pipRepositoryMock.Setup(x => x.GetByIdAsync(pipId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pip);

        // Act
        var (updatedPip, error) = await _handler.HandleAsync(command);

        // Assert
        Assert.Null(error);
        Assert.Contains("New note", updatedPip!.CheckInNotes);
        Assert.Contains("Old note", updatedPip.CheckInNotes);
    }
}
