using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the retrieval of all performance reviews for a specific employee.
/// </summary>
public class GetPerformanceReviewsQueryHandler
{
    private readonly IPerformanceReviewRepository _repository;
    private readonly IEmployeeServiceClient _employeeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPerformanceReviewsQueryHandler"/> class.
    /// </summary>
    public GetPerformanceReviewsQueryHandler(IPerformanceReviewRepository repository, IEmployeeServiceClient employeeService)
    {
        _repository = repository;
        _employeeService = employeeService;
    }

    /// <summary>
    /// Retrieves a collection of performance reviews.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of performance reviews.</returns>
    public async Task<IEnumerable<PerformanceReview>> HandleAsync(GetPerformanceReviewsQuery query, CancellationToken cancellationToken = default)
    {
        // Authorization check: Employees see own reviews, Managers see direct reports
        if (query.EmployeeId != query.RequestingUserId)
        {
            var isManager = await _employeeService.ValidateManagerEmployeeRelationshipAsync(query.RequestingUserId, query.EmployeeId, cancellationToken);
            if (!isManager)
            {
                return Enumerable.Empty<PerformanceReview>();
            }
        }

        return await _repository.GetByEmployeeIdAsync(query.EmployeeId, cancellationToken);
    }
}
