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

public class PerformanceReviewsControllerTests : BaseIntegrationTest
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
    public async Task CreateReview_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1),
            SelfAssessment = "I'm great"
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var review = await response.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();
        Assert.NotNull(review);
        Assert.Equal(employeeId, review.EmployeeId);
        Assert.Equal(ReviewStatus.Draft, review.Status);
    }

    [Fact]
    public async Task CreateReview_Overlapping_ReturnsBadRequest()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var request = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1)
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create first review
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", request);

        // Act - Try creating overlapping
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("overlapping", error);
    }

    [Fact]
    public async Task SubmitReview_ValidRequest_ReturnsOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var createRequest = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1),
            SelfAssessment = "Initial assessment"
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", createRequest);
        var createdReview = await createResponse.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();

        // Submit Self-Assessment (transition to SelfAssessmentPending)
        var updateRequest = new UpdatePerformanceReviewRequest { SubmitSelfAssessment = true, SelfAssessment = "Self" };
        await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/reviews/{createdReview!.Id}", updateRequest);

        var submitRequest = new SubmitPerformanceReviewRequest
        {
            ManagerAssessment = "Good work",
            OverallRating = PerformanceRating.MeetsExpectations
        };

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/reviews/{createdReview.Id}/submit", submitRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var submittedReview = await response.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();
        Assert.Equal(ReviewStatus.Submitted, submittedReview!.Status);
        Assert.Equal(PerformanceRating.MeetsExpectations, submittedReview.OverallRating);
    }

    [Fact]
    public async Task AcknowledgeReview_ValidRequest_UpdatesStatus()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        
        // Setup mock auth to be this employee
        TestAuthHandler.UserId = employeeId;

        var createRequest = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1),
            SelfAssessment = "Self"
        };

        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", createRequest);
        var createdReview = await createResponse.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();

        // Submit Self-Assessment
        var updateRequest = new UpdatePerformanceReviewRequest { SubmitSelfAssessment = true, SelfAssessment = "Self" };
        await _client.PutAsJsonSnakeCaseAsync($"/performance/v1/reviews/{createdReview!.Id}", updateRequest);

        // Submit Manager Review
        var submitRequest = new SubmitPerformanceReviewRequest
        {
            ManagerAssessment = "Good work",
            OverallRating = PerformanceRating.MeetsExpectations
        };
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/reviews/{createdReview.Id}/submit", submitRequest);

        // Act - Acknowledge
        var response = await _client.PostAsync($"/performance/v1/reviews/{createdReview.Id}/acknowledge", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var acknowledgedReview = await response.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();
        Assert.Equal(ReviewStatus.Acknowledged, acknowledgedReview!.Status);
        Assert.NotNull(acknowledgedReview.AcknowledgedDate);
    }
}
