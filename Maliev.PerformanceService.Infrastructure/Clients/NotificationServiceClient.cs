using System.Net.Http.Json;
using Maliev.PerformanceService.Application.Interfaces;

namespace Maliev.PerformanceService.Infrastructure.Clients;

/// <summary>
/// Implementation of the notification service client using HttpClient.
/// </summary>
public class NotificationServiceClient : INotificationServiceClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationServiceClient"/> class.
    /// </summary>
    public NotificationServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task SendReviewReminderAsync(Guid employeeId, Guid reviewId, string reminderType, CancellationToken cancellationToken = default)
    {
        var request = new { EmployeeId = employeeId, ReviewId = reviewId, ReminderType = reminderType };
        await _httpClient.PostAsJsonAsync("/api/v1/notifications/performance-review-reminder", request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendPIPCheckInReminderAsync(Guid employeeId, Guid pipId, DateTime checkInDate, CancellationToken cancellationToken = default)
    {
        var request = new { EmployeeId = employeeId, PIPId = pipId, CheckInDate = checkInDate };
        await _httpClient.PostAsJsonAsync("/api/v1/notifications/pip-checkin-reminder", request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendAcknowledgmentReminderAsync(Guid employeeId, Guid reviewId, CancellationToken cancellationToken = default)
    {
        var request = new { EmployeeId = employeeId, ReviewId = reviewId };
        await _httpClient.PostAsJsonAsync("/api/v1/notifications/review-acknowledgment-reminder", request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendGoalAtRiskAlertAsync(Guid employeeId, Guid goalId, CancellationToken cancellationToken = default)
    {
        var request = new { EmployeeId = employeeId, GoalId = goalId };
        await _httpClient.PostAsJsonAsync("/api/v1/notifications/goal-at-risk-alert", request, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SendDataVolumeWarningAsync(Guid employeeId, string entityType, int currentCount, int limit, CancellationToken cancellationToken = default)
    {
        var request = new { EmployeeId = employeeId, EntityType = entityType, CurrentCount = currentCount, Limit = limit };
        await _httpClient.PostAsJsonAsync("/api/v1/notifications/data-volume-warning", request, cancellationToken);
    }
}