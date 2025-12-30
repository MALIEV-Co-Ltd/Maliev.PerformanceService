using System.Net;
using System.Net.Http.Json;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Enums;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Maliev.PerformanceService.Tests.Integration;

[Collection("IntegrationTests")]
public class PIPsControllerTests : BaseIntegrationTest
{
    private readonly Mock<IEmployeeServiceClient> _employeeServiceMock = new();

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        _factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddScoped<IEmployeeServiceClient>(_ => _employeeServiceMock.Object);
            });
        });

        _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Test");
    }

    [Fact]
    public async Task CreatePIP_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreatePIPRequest
        {
            EmployeeId = employeeId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Reason = "Poor performance",
            ImprovementAreas = "Coding speed",
            SuccessCriteria = "Complete Jira tickets on time"
        };

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/pips", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var pip = await response.Content.ReadFromJsonSnakeCaseAsync<PIPDto>();
        Assert.NotNull(pip);
        Assert.Equal(PIPStatus.Active, pip.Status);
    }

    [Fact]
    public async Task CreatePIP_DuplicateActive_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreatePIPRequest
        {
            EmployeeId = employeeId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            Reason = "Reason",
            ImprovementAreas = "Areas",
            SuccessCriteria = "Criteria"
        };

        // Create first PIP
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/pips", request);

        // Act - Try creating second PIP
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/pips", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("already has an active PIP", error);
    }
}
