namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the type of feedback provider.
/// </summary>
public enum FeedbackType
{
    /// <summary>
    /// Feedback provided by a manager.
    /// </summary>
    Manager = 0,

    /// <summary>
    /// Feedback provided by a peer.
    /// </summary>
    Peer = 1,

    /// <summary>
    /// Feedback provided by a direct report.
    /// </summary>
    DirectReport = 2,

    /// <summary>
    /// Self-assessment feedback.
    /// </summary>
    Self = 3
}