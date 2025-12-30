using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the retrieval of a paginated list of goals for an employee.
/// </summary>
public class GetGoalsQueryHandler
{
    private readonly IGoalRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGoalsQueryHandler"/> class.
    /// </summary>
    public GetGoalsQueryHandler(IGoalRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a paginated collection of goals.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of goals and the next cursor.</returns>
    public async Task<(IEnumerable<Goal> Items, Guid? NextCursor)> HandleAsync(GetGoalsQuery query, CancellationToken cancellationToken = default)
    {
        // TODO: Authorization check
        
        return await _repository.GetByEmployeeIdPaginatedAsync(query.EmployeeId, query.Cursor, query.Limit, cancellationToken);
    }
}
