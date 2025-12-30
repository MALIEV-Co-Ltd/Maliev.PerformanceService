using System.Security.Cryptography;
using System.Text;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.PerformanceService.Infrastructure.Repositories;

/// <summary>
/// Repository for managing review feedback in the database.
/// </summary>
public class FeedbackRepository : IFeedbackRepository
{
    private readonly PerformanceDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackRepository"/> class.
    /// </summary>
    public FeedbackRepository(PerformanceDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReviewFeedback>> GetByReviewIdAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewFeedback
            .Where(x => x.PerformanceReviewId == reviewId)
            .OrderByDescending(x => x.SubmittedDate)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ReviewFeedback> CreateAsync(ReviewFeedback feedback, CancellationToken cancellationToken = default)
    {
        if (feedback.IsAnonymous)
        {
            feedback.ProviderId = HashGuid(feedback.ProviderId);
        }

        _context.ReviewFeedback.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);
        return feedback;
    }

    /// <inheritdoc/>
    public async Task<int> GetFeedbackCountByTypeAsync(Guid reviewId, int feedbackType, CancellationToken cancellationToken = default)
    {
        return await _context.ReviewFeedback
            .CountAsync(x => x.PerformanceReviewId == reviewId && (int)x.FeedbackType == feedbackType, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<int> CountByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await (from f in _context.ReviewFeedback
                      join r in _context.PerformanceReviews on f.PerformanceReviewId equals r.Id
                      where r.EmployeeId == employeeId
                      select f).CountAsync(cancellationToken);
    }

    private static Guid HashGuid(Guid guid)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(guid.ToByteArray());
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes);
    }
}