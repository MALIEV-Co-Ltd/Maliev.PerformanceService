using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PerformanceService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing performance goals in the database.
/// </summary>
public class GoalRepository : IGoalRepository
{
    private readonly PerformanceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalRepository"/> class.
    /// </summary>
    public GoalRepository(PerformanceDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Goal?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Goals.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Goal>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<(IEnumerable<Goal> Items, Guid? NextCursor)> GetByEmployeeIdPaginatedAsync(Guid employeeId, Guid? cursor, int limit, CancellationToken cancellationToken = default)
    {
        IQueryable<Goal> query = _context.Goals
            .Where(x => x.EmployeeId == employeeId);

        if (cursor.HasValue)
        {
            query = query.Where(x => x.Id.CompareTo(cursor.Value) > 0);
        }

        query = query.OrderBy(x => x.Id);

        var items = await query.Take(limit + 1).ToListAsync(cancellationToken);
        
        Guid? nextCursor = null;
        if (items.Count > limit)
        {
            nextCursor = items[limit - 1].Id;
            items = items.Take(limit).ToList();
        }

        return (items, nextCursor);
    }

    /// <inheritdoc/>
    public async Task<Goal> CreateAsync(Goal goal, CancellationToken cancellationToken = default)
    {
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync(cancellationToken);
        return goal;
    }

    /// <inheritdoc/>
    public async Task<Goal> UpdateAsync(Goal goal, CancellationToken cancellationToken = default)
    {
        _context.Entry(goal).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return goal;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var goal = await _context.Goals.FindAsync(new object[] { id }, cancellationToken);
        if (goal != null)
        {
            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Goals.CountAsync(x => x.EmployeeId == employeeId, cancellationToken);
    }
}