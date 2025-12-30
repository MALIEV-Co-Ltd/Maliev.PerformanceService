using Maliev.PerformanceService.Application.Commands;

namespace Maliev.PerformanceService.Application.Validators;

/// <summary>
/// Validator for feedback submission.
/// </summary>
public class SubmitFeedbackValidator
{
    /// <summary>
    /// Validates the submit feedback command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public (bool IsValid, string? Error) Validate(SubmitFeedbackCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Feedback))
        {
            return (false, "Feedback text is required.");
        }

        return (true, null);
    }
}
