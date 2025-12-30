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

public class FeedbackControllerTests : BaseIntegrationTest
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
    public async Task SubmitFeedback_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Create a review first
        var createReviewRequest = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1)
        };
        var reviewResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", createReviewRequest);
        var review = await reviewResponse.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();

        var feedbackRequest = new SubmitFeedbackRequest
        {
            FeedbackType = FeedbackType.Peer,
            Feedback = "Excellent performance",
            IsAnonymous = false
        };

        // Act
        var response = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/reviews/{review!.Id}/feedback", feedbackRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var feedback = await response.Content.ReadFromJsonSnakeCaseAsync<FeedbackDto>();
        Assert.NotNull(feedback);
        Assert.Equal(FeedbackType.Peer, feedback.FeedbackType);
        Assert.Equal("Excellent performance", feedback.Feedback);
    }

    [Fact]
    public async Task GetFeedback_SingleAnonymous_IsSuppressed()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        TestAuthHandler.UserId = employeeId; // Be the employee
        _employeeServiceMock.Setup(x => x.ValidateEmployeeExistsAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var createReviewRequest = new CreatePerformanceReviewRequest
        {
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = DateTime.UtcNow,
            ReviewPeriodEnd = DateTime.UtcNow.AddYears(1)
        };
        var reviewResponse = await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/employees/{employeeId}/reviews", createReviewRequest);
        var review = await reviewResponse.Content.ReadFromJsonSnakeCaseAsync<PerformanceReviewDto>();

        var feedbackRequest = new SubmitFeedbackRequest
        {
            FeedbackType = FeedbackType.Peer,
            Feedback = "I am anonymous",
            IsAnonymous = true
        };
        await _client.PostAsJsonSnakeCaseAsync($"/performance/v1/reviews/{review!.Id}/feedback", feedbackRequest);

        // Act
        var response = await _client.GetAsync($"/performance/v1/reviews/{review.Id}/feedback");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var feedbackList = await response.Content.ReadFromJsonSnakeCaseAsync<List<FeedbackDto>>();
        Assert.Empty(feedbackList!); // Suppressed because only 1 anonymous feedback
    }
}
