using Maliev.PerformanceService.Application.Commands;

namespace Maliev.PerformanceService.Application.Validators;

/// <summary>
/// Validator for creating a new PIP.
/// </summary>
public class CreatePIPValidator
{
    /// <summary>
    /// Validates the PIP creation command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public (bool IsValid, string? Error) Validate(CreatePIPCommand command)
    {
        if (command.StartDate >= command.EndDate)
        {
            return (false, "Start date must be before end date.");
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return (false, "Reason is required.");
        }

        if (string.IsNullOrWhiteSpace(command.ImprovementAreas))
        {
            return (false, "Improvement areas are required.");
        }

        if (string.IsNullOrWhiteSpace(command.SuccessCriteria))
        {
            return (false, "Success criteria are required.");
        }

        return (true, null);
    }
}
