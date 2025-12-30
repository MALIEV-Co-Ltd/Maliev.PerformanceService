using Maliev.PerformanceService.Application.Commands;

namespace Maliev.PerformanceService.Application.Validators;

/// <summary>
/// Provides validation logic for creating a new goal.
/// </summary>
public class CreateGoalValidator
{
    /// <summary>
    /// Validates the goal creation command.
    /// </summary>
    /// <param name="command">The command to validate.</param>
    /// <returns>A tuple indicating success and an optional error message.</returns>
    public (bool IsValid, string? Error) Validate(CreateGoalCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Description))
        {
            return (false, "Goal description is required.");
        }

        if (command.TargetCompletionDate <= DateTime.UtcNow)
        {
            return (false, "Target completion date must be in the future.");
        }

        return (true, null);
    }
}