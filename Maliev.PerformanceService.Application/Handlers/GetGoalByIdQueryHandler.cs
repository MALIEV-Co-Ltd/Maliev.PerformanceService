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
    private readonly IEmployeeServiceClient _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetGoalByIdQueryHandler"/> class.
    /// </summary>
    public GetGoalByIdQueryHandler(IGoalRepository repository, IEmployeeServiceClient employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
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
        if (goal == null)
        {
            return null;
        }

        // Authorization check: Employee sees own goals, Manager sees reports' goals
        if (goal.EmployeeId != query.RequestingUserId)
        {
            var isManager = await _employeeService.ValidateManagerEmployeeRelationshipAsync(query.RequestingUserId, goal.EmployeeId, cancellationToken);
            if (!isManager)
            {
                return null;
            }
        }

        return goal;
    }
}
