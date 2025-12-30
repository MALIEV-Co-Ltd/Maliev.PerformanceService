using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handler for GetPIPsQuery.
/// </summary>
public class GetPIPsQueryHandler
{
    private readonly IPIPRepository _pipRepository;
    private readonly ILogger<GetPIPsQueryHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetPIPsQueryHandler"/> class.
    /// </summary>
    /// <param name="pipRepository">The PIP repository.</param>
    /// <param name="logger">The logger.</param>
    public GetPIPsQueryHandler(IPIPRepository pipRepository, ILogger<GetPIPsQueryHandler> logger)
    {
        _pipRepository = pipRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the retrieval of PIPs for an employee.
    /// </summary>
    /// <param name="query">The retrieval query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of PIPs.</returns>
    public async Task<IEnumerable<PerformanceImprovementPlan>> HandleAsync(GetPIPsQuery query, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching PIPs for employee {EmployeeId}", query.EmployeeId);
        return await _pipRepository.GetByEmployeeIdAsync(query.EmployeeId, cancellationToken);
    }
}