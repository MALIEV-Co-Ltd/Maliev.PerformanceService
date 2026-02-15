using System.Net;
using System.Net.Http.Json;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Interfaces;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Integration;

[Collection("IntegrationTests")]
public class GetGoalByIdTests : BaseIntegrationTest
{
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Services:NotificationService:BaseUrl"] = "http://localhost"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IEmployeeServiceClient>(_ => _employeeServiceMock.Object);
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task GetGoalById_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var createRequest = new CreateGoalRequest
        {
            Description = "Test Description",
            TargetCompletionDate = DateTime.UtcNow.AddMonths(1)
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create a goal first
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", createRequest);
        var createdGoal = await createResponse.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();

        // Act - Switch to employee to view own goal
        TestAuthHandler.UserId = employeeId;
        var response = await _client.GetAsync($"/performance/v1/goals/{createdGoal!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var goal = await response.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();
        Assert.NotNull(goal);
        Assert.Equal(createdGoal.Id, goal.Id);
    }
}
