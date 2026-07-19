namespace Maliev.PerformanceService.Application.Authorization;

/// <summary>
/// Defines the permissions for the Performance Service.
/// </summary>
public static class PerformancePermissions
{
    public const string ReviewCreate = "performance.reviews.create";
    public const string ReviewRead = "performance.reviews.read";
    public const string ReviewUpdate = "performance.reviews.update";
    public const string ReviewSubmit = "performance.reviews.submit";
    public const string ReviewApprove = "performance.reviews.approve";

    public const string GoalCreate = "performance.goals.create";
    public const string GoalRead = "performance.goals.read";
    public const string GoalUpdate = "performance.goals.update";
    public const string GoalClose = "performance.goals.close";

    public const string ReportRead = "performance.reports.read";
    public const string ReportExport = "performance.reports.export";

    public const string CalibrationRead = "performance.calibrations.read";
    public const string CalibrationManage = "performance.calibrations.manage";

    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { ReviewCreate, "Create performance reviews" },
        { ReviewRead, "Read performance reviews" },
        { ReviewUpdate, "Update performance reviews" },
        { ReviewSubmit, "Submit performance reviews" },
        { ReviewApprove, "Approve performance reviews" },
        { GoalCreate, "Create performance goals" },
        { GoalRead, "Read performance goals" },
        { GoalUpdate, "Update performance goals" },
        { GoalClose, "Close performance goals" },
        { ReportRead, "Read performance reports" },
        { ReportExport, "Export performance reports" },
        { CalibrationRead, "Read performance calibrations" },
        { CalibrationManage, "Manage performance calibrations" },
    };

    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
