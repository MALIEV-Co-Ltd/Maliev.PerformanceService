using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Queries;
using Maliev.PerformanceService.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Maliev.PerformanceService.Api.Controllers;

/// <summary>
/// Controller for managing 360-degree feedback.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("performance/v{version:apiVersion}/reviews")]
public class FeedbackController : ControllerBase
{
    private readonly SubmitFeedbackCommandHandler _submitHandler;
    private readonly GetFeedbackQueryHandler _getFeedbackHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeedbackController"/> class.
    /// </summary>
    public FeedbackController(
        SubmitFeedbackCommandHandler submitHandler,
        GetFeedbackQueryHandler getFeedbackHandler)
    {
        _submitHandler = submitHandler;
        _getFeedbackHandler = getFeedbackHandler;
    }

    /// <summary>
    /// Submits feedback for a performance review.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <param name="request">The feedback submission details.</param>
    /// <returns>The submitted feedback details.</returns>
    [HttpPost("{reviewId}/feedback")]
    [RequirePermission(PerformancePermissions.Feedback)]
    [ProducesResponseType(typeof(FeedbackDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SubmitFeedback(Guid reviewId, [FromBody] SubmitFeedbackRequest request)
    {
        var command = new SubmitFeedbackCommand(
            reviewId,
            GetUserId(),
            request.FeedbackType,
            request.Feedback,
            request.IsAnonymous,
            GetUserId());

        var (feedback, error) = await _submitHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return CreatedAtAction(nameof(GetFeedback), new { reviewId = reviewId }, MapToDto(feedback!));
    }

    /// <summary>
    /// Retrieves aggregated feedback for a performance review.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <returns>A collection of feedback details.</returns>
    [HttpGet("{reviewId}/feedback")]
    [RequirePermission(PerformancePermissions.Feedback)]
    [ProducesResponseType(typeof(IEnumerable<FeedbackDto>), 200)]
    public async Task<IActionResult> GetFeedback(Guid reviewId)
    {
        var query = new GetFeedbackQuery(reviewId, GetUserId());
        var feedback = await _getFeedbackHandler.HandleAsync(query);
        return Ok(feedback.Select(MapToDto));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static FeedbackDto MapToDto(Domain.Entities.ReviewFeedback feedback)
    {
        return new FeedbackDto(
            feedback.Id,
            feedback.PerformanceReviewId,
            feedback.IsAnonymous ? null : feedback.ProviderId,
            feedback.FeedbackType,
            feedback.Feedback,
            feedback.IsAnonymous,
            feedback.SubmittedDate);
    }
}
