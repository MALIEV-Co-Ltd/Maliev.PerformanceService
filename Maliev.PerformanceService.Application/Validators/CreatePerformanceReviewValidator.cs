using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;

namespace Maliev.PerformanceService.Application.Validators;

/// <summary>
/// Provides validation logic for creating a new performance review.
/// </summary>
public class CreatePerformanceReviewValidator
{
    private readonly IPerformanceReviewRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePerformanceReviewValidator"/> class.
    /// </summary>
    public CreatePerformanceReviewValidator(IPerformanceReviewRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Validates the review creation command, checking for valid dates and overlaps.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public async Task<(bool IsValid, string? Error)> ValidateAsync(CreatePerformanceReviewCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ReviewPeriodStart >= command.ReviewPeriodEnd)
        {
            return (false, "Review period start must be before end.");
        }

        if (await _repository.ExistsOverlappingReviewAsync(command.EmployeeId, (int)command.ReviewCycle, command.ReviewPeriodStart, command.ReviewPeriodEnd, null, cancellationToken))
        {
            return (false, "An overlapping review period already exists for this employee.");
        }

        return (true, null);
    }
}