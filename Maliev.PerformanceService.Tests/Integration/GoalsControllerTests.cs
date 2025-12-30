using System.Net;
using System.Net.Http.Json;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Integration;

public class GoalsControllerTests : BaseIntegrationTest
{
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();
    private readonly Mock<INotificationServiceClient> _notificationServiceMock = new();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IEmployeeServiceClient>(_ => _employeeServiceMock.Object);
                services.AddScoped<INotificationServiceClient>(_ => _notificationServiceMock.Object);
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task CreateGoal_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreateGoalRequest
        {
            Description = "Learn Three.js",
            SuccessCriteria = "Build a 3D solar system",
            TargetCompletionDate = DateTime.UtcNow.AddMonths(3)
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var goal = await response.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();
        Assert.NotNull(goal);
        Assert.Equal(employeeId, goal.EmployeeId);
        Assert.Equal(GoalStatus.NotStarted, goal.CurrentStatus);
    }

    [Fact]
    public async Task UpdateProgress_ValidRequest_UpdatesStatus()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createRequest = new CreateGoalRequest
        {
            Description = "Initial Goal",
            TargetCompletionDate = DateTime.UtcNow.AddMonths(1)
        };
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", createRequest);
        var createdGoal = await createResponse.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();

        var progressRequest = new UpdateGoalProgressRequest
        {
            ProgressUpdate = "Completed first module",
            CompletionStatus = GoalStatus.InProgress
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/goals/{createdGoal!.Id}/progress", progressRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updatedGoal = await response.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();
        Assert.Equal(GoalStatus.InProgress, updatedGoal!.CurrentStatus);
        Assert.Contains("Completed first module", updatedGoal.ProgressUpdates!);
    }

    [Fact]
    public async Task UpdateProgress_AtRisk_SendsAlert()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createRequest = new CreateGoalRequest
        {
            Description = "Risk Goal",
            TargetCompletionDate = DateTime.UtcNow.AddMonths(1)
        };
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", createRequest);
        var createdGoal = await createResponse.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();

        // Transition to InProgress first to satisfy validator
        var inProgressRequest = new UpdateGoalProgressRequest
        {
            ProgressUpdate = "Starting",
            CompletionStatus = GoalStatus.InProgress
        };
        await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/goals/{createdGoal!.Id}/progress", inProgressRequest);

        var progressRequest = new UpdateGoalProgressRequest
        {
            ProgressUpdate = "Stuck on a bug",
            CompletionStatus = GoalStatus.AtRisk
        };

        // Act
        var response = await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/goals/{createdGoal.Id}/progress", progressRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _notificationServiceMock.Verify(x => x.SendGoalAtRiskAlertAsync(employeeId, createdGoal.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
