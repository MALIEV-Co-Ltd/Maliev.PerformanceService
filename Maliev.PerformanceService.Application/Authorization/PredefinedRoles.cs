namespace Maliev.PerformanceService.Application.Authorization;

/// <summary>
/// Provides access to predefined roles for the Performance Service.
/// </summary>
public static class PerformancePredefinedRoles
{
    public const string Admin = "roles.performance.admin";
    public const string Manager = "roles.performance.manager";
    public const string Viewer = "roles.performance.viewer";

    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (
            Admin,
            "Performance Administrator with full access",
            new[]
            {
                PerformancePermissions.ReviewCreate,
                PerformancePermissions.ReviewRead,
                PerformancePermissions.ReviewUpdate,
                PerformancePermissions.ReviewSubmit,
                PerformancePermissions.ReviewApprove,
                PerformancePermissions.GoalCreate,
                PerformancePermissions.GoalRead,
                PerformancePermissions.GoalUpdate,
                PerformancePermissions.GoalClose,
                PerformancePermissions.ReportRead,
                PerformancePermissions.ReportExport,
                PerformancePermissions.CalibrationRead,
                PerformancePermissions.CalibrationManage,
            }
        ),
        (
            Manager,
            "Performance Manager with review and goal access",
            new[]
            {
                PerformancePermissions.ReviewCreate,
                PerformancePermissions.ReviewRead,
                PerformancePermissions.ReviewUpdate,
                PerformancePermissions.ReviewSubmit,
                PerformancePermissions.ReviewApprove,
                PerformancePermissions.GoalCreate,
                PerformancePermissions.GoalRead,
                PerformancePermissions.GoalUpdate,
                PerformancePermissions.GoalClose,
                PerformancePermissions.ReportRead,
                PerformancePermissions.CalibrationRead,
            }
        ),
        (
            Viewer,
            "Performance Viewer with read-only access",
            new[]
            {
                PerformancePermissions.ReviewRead,
                PerformancePermissions.GoalRead,
                PerformancePermissions.ReportRead,
                PerformancePermissions.CalibrationRead,
            }
        ),
    };
}
