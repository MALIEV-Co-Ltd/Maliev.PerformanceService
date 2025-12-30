using System.Security.Claims;
using Maliev.PerformanceService.Api.DTOs;
using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Handlers;
using Maliev.PerformanceService.Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.PerformanceService.Api.Controllers;

/// <summary>
/// Controller for managing performance reviews.
/// </summary>
[Authorize]
[ApiController]
[Route("performance/v1")]
public class PerformanceReviewsController : ControllerBase
{
    private readonly CreatePerformanceReviewCommandHandler _createHandler;
    private readonly UpdatePerformanceReviewCommandHandler _updateHandler;
    private readonly SubmitPerformanceReviewCommandHandler _submitHandler;
    private readonly AcknowledgePerformanceReviewCommandHandler _acknowledgeHandler;
    private readonly GetPerformanceReviewsQueryHandler _getReviewsHandler;
    private readonly GetPerformanceReviewByIdQueryHandler _getReviewByIdHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceReviewsController"/> class.
    /// </summary>
    public PerformanceReviewsController(
        CreatePerformanceReviewCommandHandler createHandler,
        UpdatePerformanceReviewCommandHandler updateHandler,
        SubmitPerformanceReviewCommandHandler submitHandler,
        AcknowledgePerformanceReviewCommandHandler acknowledgeHandler,
        GetPerformanceReviewsQueryHandler getReviewsHandler,
        GetPerformanceReviewByIdQueryHandler getReviewByIdHandler)
    {
        _createHandler = createHandler;
        _updateHandler = updateHandler;
        _submitHandler = submitHandler;
        _acknowledgeHandler = acknowledgeHandler;
        _getReviewsHandler = getReviewsHandler;
        _getReviewByIdHandler = getReviewByIdHandler;
    }

    /// <summary>
    /// Creates a new performance review for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <param name="request">The creation request details.</param>
    /// <returns>The created review details.</returns>
    [HttpPost("employees/{employeeId}/reviews")]
    [ProducesResponseType(typeof(PerformanceReviewDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateReview(Guid employeeId, [FromBody] CreatePerformanceReviewRequest request)
    {
        var command = new CreatePerformanceReviewCommand(
            employeeId,
            request.ReviewCycle,
            request.ReviewPeriodStart,
            request.ReviewPeriodEnd,
            request.SelfAssessment,
            GetUserId());

        var (review, error) = await _createHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return CreatedAtAction(nameof(GetReviewById), new { reviewId = review!.Id }, MapToDto(review));
    }

    /// <summary>
    /// Retrieves all performance reviews for a specific employee.
    /// </summary>
    /// <param name="employeeId">The employee identifier.</param>
    /// <returns>A list of performance reviews.</returns>
    [HttpGet("employees/{employeeId}/reviews")]
    [ProducesResponseType(typeof(IEnumerable<PerformanceReviewDto>), 200)]
    public async Task<IActionResult> GetReviews(Guid employeeId)
    {
        var query = new GetPerformanceReviewsQuery(employeeId, GetUserId());
        var reviews = await _getReviewsHandler.HandleAsync(query);
        return Ok(reviews.Select(MapToDto));
    }

    /// <summary>
    /// Retrieves a specific performance review by its identifier.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <returns>The performance review details.</returns>
    [HttpGet("reviews/{reviewId}")]
    [ProducesResponseType(typeof(PerformanceReviewDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetReviewById(Guid reviewId)
    {
        var query = new GetPerformanceReviewByIdQuery(reviewId, GetUserId());
        var review = await _getReviewByIdHandler.HandleAsync(query);
        if (review == null)
        {
            return NotFound();
        }
        return Ok(MapToDto(review));
    }

    /// <summary>
    /// Updates an existing performance review, typically for saving self-assessment drafts.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <param name="request">The update request details.</param>
    /// <returns>The updated performance review details.</returns>
    [HttpPut("reviews/{reviewId}")]
    [ProducesResponseType(typeof(PerformanceReviewDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdatePerformanceReviewRequest request)
    {
        var command = new UpdatePerformanceReviewCommand(
            reviewId,
            request.SelfAssessment,
            request.ManagerAssessment,
            request.SubmitSelfAssessment,
            GetUserId());

        var (review, error) = await _updateHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(review!));
    }

    /// <summary>
    /// Submits a manager assessment and overall rating for a review cycle.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <param name="request">The submission request details.</param>
    /// <returns>The updated performance review details.</returns>
    [HttpPost("reviews/{reviewId}/submit")]
    [ProducesResponseType(typeof(PerformanceReviewDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SubmitReview(Guid reviewId, [FromBody] SubmitPerformanceReviewRequest request)
    {
        var command = new SubmitPerformanceReviewCommand(
            reviewId,
            request.ManagerAssessment,
            request.OverallRating,
            GetUserId());

        var (review, error) = await _submitHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(review!));
    }

    /// <summary>
    /// Acknowledges a performance review by the employee.
    /// </summary>
    /// <param name="reviewId">The performance review identifier.</param>
    /// <returns>The updated performance review details.</returns>
    [HttpPost("reviews/{reviewId}/acknowledge")]
    [ProducesResponseType(typeof(PerformanceReviewDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AcknowledgeReview(Guid reviewId)
    {
        var command = new AcknowledgePerformanceReviewCommand(reviewId, GetUserId());
        var (review, error) = await _acknowledgeHandler.HandleAsync(command);
        if (error != null)
        {
            return BadRequest(new { Message = error });
        }

        return Ok(MapToDto(review!));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return userIdClaim != null ? Guid.Parse(userIdClaim) : Guid.Empty;
    }

    private static PerformanceReviewDto MapToDto(Domain.Entities.PerformanceReview review)
    {
        return new PerformanceReviewDto(
            review.Id,
            review.EmployeeId,
            review.ReviewerId,
            review.ReviewCycle,
            review.ReviewPeriodStart,
            review.ReviewPeriodEnd,
            review.SelfAssessment,
            review.ManagerAssessment,
            review.OverallRating,
            review.Status,
            review.SubmittedDate,
            review.AcknowledgedDate);
    }
}
