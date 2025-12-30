using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.PerformanceService.Domain.Authorization;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Infrastructure.IAM;

/// <summary>
/// Hosted service that registers performance management permissions with the IAM service on startup.
/// </summary>
public class PerformanceIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceIAMRegistrationService"/> class.
    /// </summary>
    public PerformanceIAMRegistrationService(IHttpClientFactory httpClientFactory, ILogger<PerformanceIAMRegistrationService> logger)
        : base(httpClientFactory, logger, "PerformanceService")
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return new[]
        {
            new PermissionRegistration { PermissionId = PerformancePermissions.Create, Description = "Allow creating performance reviews and goals" },
            new PermissionRegistration { PermissionId = PerformancePermissions.Read, Description = "Allow reading performance data" },
            new PermissionRegistration { PermissionId = PerformancePermissions.Update, Description = "Allow updating performance reviews and goals" },
            new PermissionRegistration { PermissionId = PerformancePermissions.Admin, Description = "Full administrative access to performance management" },
            new PermissionRegistration { PermissionId = PerformancePermissions.Feedback, Description = "Allow providing and viewing feedback" }
        };
    }

    /// <inheritdoc/>
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return Enumerable.Empty<RoleRegistration>();
    }
}