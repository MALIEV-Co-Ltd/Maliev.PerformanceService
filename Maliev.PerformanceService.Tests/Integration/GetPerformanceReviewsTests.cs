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
public class GetPerformanceReviewsTests : BaseIntegrationTest
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
    public async Task GetReviews_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var createRequest = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1),
            SelfAssessment = "Self Assessment"
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create a review first
        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", createRequest);

        // Act - Switch to employee to view own reviews
        TestAuthHandler.UserId = employeeId;
        var response = await _client.GetAsync($"/performance/v1/employees/{employeeId}/reviews");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reviews = await response.Content.ReadFromJsonSnakeCaseAsync<List<PerformanceReviewDto>>();
        Assert.NotNull(reviews);
        Assert.Contains(reviews, r => r.EmployeeId == employeeId);
    }
}
