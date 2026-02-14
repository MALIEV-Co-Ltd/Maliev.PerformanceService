using System.Net;
using System.Net.Http.Json;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Integration;

[Collection("IntegrationTests")]
public class PerformanceServiceRemainingTests : BaseIntegrationTest
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
    public async Task GetGoals_ReturnsOk()
    {
        var employeeId = BaseIntegrationTest.TestAuthHandler.UserId;
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", new CreateGoalRequest { Description = "G", TargetCompletionDate = DateTime.UtcNow.AddDays(1) });
        
        var response = await _client.GetAsync($"/performance/v1/employees/{employeeId}/goals");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPIPs_ReturnsOk()
    {
        var employeeId = BaseIntegrationTest.TestAuthHandler.UserId;
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/pips", new CreatePIPRequest { Reason = "R", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddMonths(1) });
        
        var response = await _client.GetAsync($"/performance/v1/employees/{employeeId}/pips");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetReviewById_ReturnsOk()
    {
        var employeeId = BaseIntegrationTest.TestAuthHandler.UserId;
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", new CreatePerformanceReviewRequest { ReviewCycle = ReviewCycle.Annual, ReviewPeriodStart = DateTime.UtcNow, ReviewPeriodEnd = DateTime.UtcNow.AddYears(1) });
        var created = await createResponse.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();
        
        var response = await _client.GetAsync($"/performance/v1/reviews/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateGoal_ReturnsOk()
    {
        var employeeId = BaseIntegrationTest.TestAuthHandler.UserId;
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/goals", new CreateGoalRequest { Description = "G", TargetCompletionDate = DateTime.UtcNow.AddDays(1) });
        var created = await createResponse.Content.ReadFromJsonSnakeCaseAsync<GoalDto>();
        
        var response = await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/goals/{created!.Id}/progress", new UpdateGoalProgressRequest { ProgressUpdate = "P", CompletionStatus = GoalStatus.InProgress });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
