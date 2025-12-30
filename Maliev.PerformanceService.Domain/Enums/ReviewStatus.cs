namespace Maliev.PerformanceService.Domain.Enums;

/// <summary>
/// Defines the current workflow status of a performance review.
/// </summary>
public enum ReviewStatus
{
    /// <summary>
    /// Review is in draft mode.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Review is waiting for the employee to complete their self-assessment.
    /// </summary>
    SelfAssessmentPending = 1,

    /// <summary>
    /// Review is waiting for the manager to complete their assessment.
    /// </summary>
    ManagerReviewPending = 2,

    /// <summary>
    /// Review has been submitted by the manager.
    /// </summary>
    Submitted = 3,

    /// <summary>
    /// Review has been acknowledged by the employee.
    /// </summary>
    Acknowledged = 4,

    /// <summary>
    /// Review process is fully completed.
    /// </summary>
    Completed = 5
}