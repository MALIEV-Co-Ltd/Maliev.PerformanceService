using System.Security.Claims;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.PerformanceService.Api.Controllers;

/// <summary>
/// Controller for managing Performance Improvement Plans (PIPs).
/// </summary>
[Authorize]
[ApiController]
[Route("performance/v1")]
public class PIPsController : ControllerBase
{
    private readonly CreatePIPCommandHandler _createHandler;
    private readonly UpdatePIPCommandHandler _updateHandler;
    private readonly RecordPIPOutcomeCommandHandler _outcomeHandler;
    private readonly GetPIPsQueryHandler _getPIPsHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PIPsController"/> class.
    /// </summary>
    /// <param name="createHandler">The create PIP handler.</param>
    /// <param name="updateHandler">The update PIP handler.</param>
    /// <param name="outcomeHandler">The record outcome handler.</param>
    /// <param name="getPIPsHandler">The get PIPs handler.</param>
    public PIPsController(
        CreatePIPCommandHandler createHandler,
        UpdatePIPCommandHandler updateHandler,
        RecordPIPOutcomeCommandHandler outcomeHandler,
        GetPIPsQueryHandler getPIPsHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _outcomeHandler = outcomeHandler;
        _getPIPsHandler = getPIPsHandler;
    }

    /// <summary>
    /// Creates a new PIP for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="request">The creation request.</param>
    /// <returns>A 201 Created response with the new PIP.</returns>
    [HttpPost("employees/{employeeId}/pips")]
    public async Task<IActionResult> CreatePIP(Guid employeeId, [FromBody] CreatePIPRequest request)
    {
        var command = new CreatePIPCommand(
            employeeId,
            request.StartDate,
            request.EndDate,
            request.Reason,
            request.ImprovementAreas,
            request.SuccessCriteria,
            GetUserId());

        var (pip, error) = await _createHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return CreatedAtAction(nameof(GetPIPs), new { employeeId = employeeId }, MapToDto(pip!));
    }

    /// <summary>
    /// Gets all PIPs for an employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <returns>A collection of PIPs.</returns>
    [HttpGet("employees/{employeeId}/pips")]
    public async Task<IActionResult> GetPIPs(Guid employeeId)
    {
        var query = new GetPIPsQuery(employeeId, GetUserId());
        var pips = await _getPIPsHandler.HandleAsync(query);
        return Ok(pips.Select(MapToDto));
    }

    /// <summary>
    /// Updates an existing PIP with check-in notes.
    /// </summary>
    /// <param name="pipId">The PIP identifier.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated PIP.</returns>
    [HttpPut("pips/{pipId}")]
    public async Task<IActionResult> UpdatePIP(Guid pipId, [FromBody] UpdatePIPRequest request)
    {
        var command = new UpdatePIPCommand(pipId, request.CheckInNote ?? "", GetUserId());
        var (pip, error) = await _updateHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(pip!));
    }

    /// <summary>
    /// Records the final outcome of a PIP.
    /// </summary>
    /// <param name="pipId">The PIP identifier.</param>
    /// <param name="request">The outcome request.</param>
    /// <returns>The updated PIP.</returns>
    [HttpPost("pips/{pipId}/outcome")]
    public async Task<IActionResult> RecordOutcome(Guid pipId, [FromBody] RecordPIPOutcomeRequest request)
    {
        var command = new RecordPIPOutcomeCommand(pipId, request.Outcome, request.ExtendedEndDate, GetUserId());
        var (pip, error) = await _outcomeHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(pip!));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static PIPDto MapToDto(Domain.Entities.PerformanceImprovementPlan pip)
    {
        return new PIPDto(
            pip.Id,
            pip.EmployeeId,
            pip.InitiatorId,
            pip.StartDate,
            pip.EndDate,
            pip.Reason,
            pip.ImprovementAreas,
            pip.SuccessCriteria,
            pip.CheckInNotes,
            pip.Status,
            pip.Outcome,
            pip.ExtensionCount);
    }
}