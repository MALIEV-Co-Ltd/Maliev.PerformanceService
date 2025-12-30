using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Entities;

/// <summary>
/// Represents individual feedback provided for a performance review.
/// </summary>
public class ReviewFeedback
{
    /// <summary>
    /// Gets or sets the unique identifier for the feedback entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the performance review this feedback belongs to.
    /// </summary>
    public Guid PerformanceReviewId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the person providing the feedback.
    /// If IsAnonymous is true, this ID is hashed or masked in outputs.
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// Gets or sets the type of feedback (Manager, Peer, etc.).
    /// </summary>
    public FeedbackType FeedbackType { get; set; }

    /// <summary>
    /// Gets or sets the actual feedback text.
    /// </summary>
    public string Feedback { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this feedback is provided anonymously.
    /// </summary>
    public bool IsAnonymous { get; set; }

    /// <summary>
    /// Gets or sets the date when the feedback was submitted.
    /// </summary>
    public DateTime SubmittedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the feedback record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
}