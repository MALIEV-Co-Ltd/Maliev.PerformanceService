namespace Maliev.PerformanceService.Domain.Authorization;

/// <summary>
/// Provides constant strings for all performance-related permissions.
/// </summary>
public static class PerformancePermissions
{
    /// <summary>
    /// Permission to create performance reviews and goals.
    /// </summary>
    public const string Create = "performance.reviews.create";

    /// <summary>
    /// Permission to read performance data.
    /// </summary>
    public const string Read = "performance.reviews.read";

    /// <summary>
    /// Permission to update performance reviews and goals.
    /// </summary>
    public const string Update = "performance.reviews.update";

    /// <summary>
    /// Full administrative access to performance management.
    /// </summary>
    public const string Admin = "performance.admin.manage";

    /// <summary>
    /// Permission to provide and view 360-degree feedback.
    /// </summary>
    public const string Feedback = "performance.feedback.manage";
}
