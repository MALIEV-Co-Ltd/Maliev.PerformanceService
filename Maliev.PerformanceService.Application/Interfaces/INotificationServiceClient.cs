namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Client for interacting with the Notification microservice.
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// Sends a reminder for a performance review cycle.
    /// </summary>
    /// <param name="employeeId">The recipient employee identifier.</param>
    /// <param name="reviewId">The review identifier.</param>
    /// <param name="reminderType">The type of reminder (e.g., SelfAssessment, ManagerReview).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendReviewReminderAsync(Guid employeeId, Guid reviewId, string reminderType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reminder for a PIP check-in meeting.
    /// </summary>
    /// <param name="employeeId">The recipient employee identifier.</param>
    /// <param name="pipId">The PIP identifier.</param>
    /// <param name="checkInDate">The date of the scheduled check-in.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendPIPCheckInReminderAsync(Guid employeeId, Guid pipId, DateTime checkInDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a reminder to an employee to acknowledge their completed review.
    /// </summary>
    /// <param name="employeeId">The recipient employee identifier.</param>
    /// <param name="reviewId">The review identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAcknowledgmentReminderAsync(Guid employeeId, Guid reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an alert to a manager when a goal is marked as AtRisk.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="goalId">The goal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendGoalAtRiskAlertAsync(Guid employeeId, Guid goalId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a warning to HR when an employee is approaching data volume limits.
    /// </summary>
    Task SendDataVolumeWarningAsync(Guid employeeId, string entityType, int currentCount, int limit, CancellationToken cancellationToken = default);
}