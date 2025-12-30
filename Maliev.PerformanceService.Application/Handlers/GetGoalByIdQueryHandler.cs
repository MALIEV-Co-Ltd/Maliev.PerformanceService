using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the retrieval of a specific goal by its identifier.
/// </summary>
public class GetGoalByIdQueryHandler
{
    private readonly IGoalRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGoalByIdQueryHandler"/> class.
    /// </summary>
    public GetGoalByIdQueryHandler(IGoalRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Retrieves a goal record.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The goal or null if not found.</returns>
    public async Task<Goal?> HandleAsync(GetGoalByIdQuery query, CancellationToken cancellationToken = default)
    {
        var goal = await _repository.GetByIdAsync(query.GoalId, cancellationToken);
        
        // TODO: Authorization check
        
        return goal;
    }
}
