using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Repository for managing Performance Improvement Plans (PIPs).
/// </summary>
public interface IPIPRepository
{
    /// <summary>
    /// Gets a PIP by its unique identifier.
    /// </summary>
    /// <param name="id">The PIP identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The PIP entity or null if not found.</returns>
    Task<PerformanceImprovementPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all PIPs assigned to a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of PIPs.</returns>
    Task<IEnumerable<PerformanceImprovementPlan>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new PIP record.
    /// </summary>
    /// <param name="pip">The PIP entity to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created PIP entity.</returns>
    Task<PerformanceImprovementPlan> CreateAsync(PerformanceImprovementPlan pip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing PIP record.
    /// </summary>
    /// <param name="pip">The PIP entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated PIP entity.</returns>
    Task<PerformanceImprovementPlan> UpdateAsync(PerformanceImprovementPlan pip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current active PIP for an employee, if one exists.
    /// Active means status is Active or Extended.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active PIP entity or null if none exist.</returns>
    Task<PerformanceImprovementPlan?> GetActivePIPByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active PIPs across all employees.
    /// </summary>
    Task<IEnumerable<PerformanceImprovementPlan>> GetAllActivePIPsAsync(CancellationToken cancellationToken = default);
}