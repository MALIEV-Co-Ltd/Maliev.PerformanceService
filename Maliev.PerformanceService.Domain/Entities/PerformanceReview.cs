using Maliev.PerformanceService.Domain.Enums;

namespace Maliev.PerformanceService.Domain.Entities;

/// <summary>
/// Represents a formal performance review cycle for an employee.
/// </summary>
public class PerformanceReview
{
    /// <summary>
    /// Gets or sets the unique identifier for the review.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the employee being reviewed.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the primary reviewer (usually the manager).
    /// </summary>
    public Guid ReviewerId { get; set; }

    /// <summary>
    /// Gets or sets the review cycle type (e.g., Annual, Probation).
    /// </summary>
    public ReviewCycle ReviewCycle { get; set; }

    /// <summary>
    /// Gets or sets the start date of the period being reviewed.
    /// </summary>
    public DateTime ReviewPeriodStart { get; set; }

    /// <summary>
    /// Gets or sets the end date of the period being reviewed.
    /// </summary>
    public DateTime ReviewPeriodEnd { get; set; }

    /// <summary>
    /// Gets or sets the employee's self-assessment comments.
    /// </summary>
    public string? SelfAssessment { get; set; }

    /// <summary>
    /// Gets or sets the manager's assessment comments.
    /// </summary>
    public string? ManagerAssessment { get; set; }

    /// <summary>
    /// Gets or sets the overall performance rating assigned.
    /// </summary>
    public PerformanceRating? OverallRating { get; set; }

    /// <summary>
    /// Gets or sets the current status of the review process.
    /// </summary>
    public ReviewStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date when the review was submitted by the manager.
    /// </summary>
    public DateTime? SubmittedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the employee acknowledged the review.
    /// </summary>
    public DateTime? AcknowledgedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the review record was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the review record was last modified.
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this review has been archived.
    /// </summary>
    public bool IsArchived { get; set; }
}