using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Interfaces;

/// <summary>
/// Repository for managing performance goals.
/// </summary>
public interface IGoalRepository
{
    /// <summary>
    /// Gets a goal by its unique identifier.
    /// </summary>
    /// <param name="id">The goal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The goal entity or null if not found.</returns>
    Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all goals assigned to a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of goals.</returns>
    Task<IEnumerable<Goal>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of goals assigned to a specific employee using cursor-based pagination.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cursor">The cursor for pagination.</param>
    /// <param name="limit">The maximum number of items to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of goals and the next cursor.</returns>
    Task<(IEnumerable<Goal> Items, Guid? NextCursor)> GetByEmployeeIdPaginatedAsync(Guid employeeId, Guid? cursor, int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new performance goal.
    /// </summary>
    /// <param name="goal">The goal entity to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created goal entity.</returns>
    Task<Goal> CreateAsync(Goal goal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing performance goal.
    /// </summary>
    /// <param name="goal">The goal entity with updated values.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated goal entity.</returns>
    Task<Goal> UpdateAsync(Goal goal, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a goal by its identifier.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts total goals for an employee.
    /// </summary>
    Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
}