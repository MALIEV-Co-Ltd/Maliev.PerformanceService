using System.Security.Claims;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.PerformanceService.Api.Controllers;

/// <summary>
/// Controller for managing performance goals.
/// </summary>
[Authorize]
[ApiController]
[Route("performance/v1")]
public class GoalsController : ControllerBase
{
    private readonly CreateGoalCommandHandler _createHandler;
    private readonly UpdateGoalCommandHandler _updateHandler;
    private readonly UpdateGoalProgressCommandHandler _progressHandler;
    private readonly GetGoalsQueryHandler _getGoalsHandler;
    private readonly GetGoalByIdQueryHandler _getGoalByIdHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoalsController"/> class.
    /// </summary>
    public GoalsController(
        CreateGoalCommandHandler createHandler,
        UpdateGoalCommandHandler updateHandler,
        UpdateGoalProgressCommandHandler progressHandler,
        GetGoalsQueryHandler getGoalsHandler,
        GetGoalByIdQueryHandler getGoalByIdHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _progressHandler = progressHandler;
        _getGoalsHandler = getGoalsHandler;
        _getGoalByIdHandler = getGoalByIdHandler;
    }

    /// <summary>
    /// Creates a new performance goal for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="request">The goal creation details.</param>
    /// <returns>The created goal details.</returns>
    [HttpPost("employees/{employeeId}/goals")]
    [ProducesResponseType(typeof(GoalDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateGoal(Guid employeeId, [FromBody] CreateGoalRequest request)
    {
        var command = new CreateGoalCommand(
            employeeId,
            request.Description,
            request.SuccessCriteria,
            request.TargetCompletionDate,
            request.PerformanceReviewId,
            GetUserId());

        var (goal, error) = await _createHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return CreatedAtAction(nameof(GetGoalById), new { goalId = goal!.Id }, MapToDto(goal));
    }

    /// <summary>
    /// Retrieves a paginated list of goals for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="cursor">Optional pagination cursor.</param>
    /// <param name="limit">Maximum number of items to return.</param>
    /// <returns>A paginated list of goals.</returns>
    [HttpGet("employees/{employeeId}/goals")]
    [ProducesResponseType(typeof(IEnumerable<GoalDto>), 200)]
    public async Task<IActionResult> GetGoals(Guid employeeId, [FromQuery] Guid? cursor, [FromQuery] int limit = 10)
    {
        var query = new GetGoalsQuery(employeeId, cursor, limit, GetUserId());
        var (items, nextCursor) = await _getGoalsHandler.HandleAsync(query);
        
        Response.Headers["X-Next-Cursor"] = nextCursor?.ToString();
        return Ok(items.Select(MapToDto));
    }

    /// <summary>
    /// Retrieves a specific goal by its identifier.
    /// </summary>
    /// <param name="goalId">The goal identifier.</param>
    /// <returns>The goal details.</returns>
    [HttpGet("goals/{goalId}")]
    [ProducesResponseType(typeof(GoalDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetGoalById(Guid goalId)
    {
        var query = new GetGoalByIdQuery(goalId, GetUserId());
        var goal = await _getGoalByIdHandler.HandleAsync(query);
        if (goal == null)
        {
            return NotFound();
        }
        return Ok(MapToDto(goal));
    }

    /// <summary>
    /// Updates the core details of an existing goal.
    /// </summary>
    /// <param name="goalId">The goal identifier.</param>
    /// <param name="request">The update details.</param>
    /// <returns>The updated goal details.</returns>
    [HttpPut("goals/{goalId}")]
    [ProducesResponseType(typeof(GoalDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] CreateGoalRequest request)
    {
        var command = new UpdateGoalCommand(
            goalId,
            request.Description,
            request.SuccessCriteria,
            request.TargetCompletionDate,
            GetUserId());

        var (goal, error) = await _updateHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(goal!));
    }

    /// <summary>
    /// Records a timestamped progress update and current status for a goal.
    /// </summary>
    /// <param name="goalId">The goal identifier.</param>
    /// <param name="request">The progress update details.</param>
    /// <returns>The updated goal details.</returns>
    [HttpPut("goals/{goalId}/progress")]
    [ProducesResponseType(typeof(GoalDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateProgress(Guid goalId, [FromBody] UpdateGoalProgressRequest request)
    {
        var command = new UpdateGoalProgressCommand(
            goalId,
            request.ProgressUpdate,
            request.CompletionStatus,
            GetUserId());

        var (goal, error) = await _progressHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(goal!));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static GoalDto MapToDto(Domain.Entities.Goal goal)
    {
        return new GoalDto(
            goal.Id,
            goal.EmployeeId,
            goal.PerformanceReviewId,
            goal.Description,
            goal.SuccessCriteria,
            goal.TargetCompletionDate,
            goal.CurrentStatus,
            goal.ProgressUpdates,
            goal.CompletionDate);
    }
}
