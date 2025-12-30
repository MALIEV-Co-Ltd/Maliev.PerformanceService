using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Maliev.PerformanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PerformanceService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing performance reviews in the database.
/// </summary>
public class PerformanceReviewRepository : IPerformanceReviewRepository
{
    private readonly PerformanceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewRepository"/> class.
    /// </summary>
    public PerformanceReviewRepository(PerformanceDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<PerformanceReview?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PerformanceReview>> GetByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PerformanceReview> CreateAsync(PerformanceReview review, CancellationToken cancellationToken = default)
    {
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);
        return review;
    }

    /// <inheritdoc/>
    public async Task<PerformanceReview> UpdateAsync(PerformanceReview review, CancellationToken cancellationToken = default)
    {
        _context.Entry(review).State = EntityState.Modified;
        await _context.SaveChangesAsync(cancellationToken);
        return review;
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _context.PerformanceReviews.FindAsync(new object[] { id }, cancellationToken);
        if (review != null)
        {
            _context.PerformanceReviews.Remove(review);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsOverlappingReviewAsync(Guid employeeId, int reviewCycle, DateTime periodStart, DateTime periodEnd, Guid? excludeReviewId = null, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews
            .Where(x => x.EmployeeId == employeeId && (int)x.ReviewCycle == reviewCycle && x.Status != ReviewStatus.Completed)
            .Where(x => excludeReviewId == null || x.Id != excludeReviewId)
            .AnyAsync(x => x.ReviewPeriodStart < periodEnd && periodStart < x.ReviewPeriodEnd, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PerformanceReview>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews
            .Where(x => x.Status == ReviewStatus.SelfAssessmentPending || x.Status == ReviewStatus.ManagerReviewPending)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews
            .CountAsync(x => x.EmployeeId == employeeId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<PerformanceReview>> GetReviewsForArchivalAsync(DateTime olderThan, CancellationToken cancellationToken = default)
    {
        return await _context.PerformanceReviews
            .Where(x => x.CreatedDate < olderThan && !x.IsArchived)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task MarkAsArchivedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var review = await _context.PerformanceReviews.FindAsync(new object[] { id }, cancellationToken);
        if (review != null)
        {
            review.IsArchived = true;
            review.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}