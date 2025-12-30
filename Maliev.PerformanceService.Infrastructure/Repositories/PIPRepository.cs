using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PerformanceService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing Performance Improvement Plans (PIPs) in the database.
/// </summary>
public class PIPRepository : IPIPRepository
{
    private readonly PerformanceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PIPRepository"/> class.
    /// </summary>
    public PIPRepository(PerformanceDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PerformanceImprovementPlan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceImprovementPlans.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PerformanceImprovementPlan>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceImprovementPlans
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PerformanceImprovementPlan> CreateAsync(PerformanceImprovementPlan pip, CancellationToken cancellationToken = default)
    {
        _context.PerformanceImprovementPlans.Add(pip);
        await _context.SaveChangesAsync(cancellationToken);
        return pip;
    }

    /// <inheritdoc/>
    public async Task<PerformanceImprovementPlan> UpdateAsync(PerformanceImprovementPlan pip, CancellationToken cancellationToken = default)
    {
        _context.Entry(pip).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return pip;
    }

    /// <inheritdoc/>
    public async Task<PerformanceImprovementPlan?> GetActivePIPByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceImprovementPlans
            .Where(x => x.EmployeeId == employeeId && (x.Status == PIPStatus.Active || x.Status == PIPStatus.Extended))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PerformanceImprovementPlan>> GetAllActivePIPsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceImprovementPlans
            .Where(x => x.Status == PIPStatus.Active || x.Status == PIPStatus.Extended)
            .ToListAsync(cancellationToken);
    }
}