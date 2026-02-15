using Maliev.PerformanceService.Application.Commands;
using Maliev.PerformanceService.Application.Interfaces;
using Maliev.PerformanceService.Application.Validators;
using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using MassTransit;

namespace Maliev.PerformanceService.Application.Handlers;

/// <summary>
/// Handles the creation of a new performance review cycle.
/// </summary>
public class CreatePerformanceReviewCommandHandler
{
    private readonly IPerformanceReviewRepository _repository;
    private readonly IEmployeeServiceClient _employeeService;
    private readonly INotificationServiceClient _notificationService;
    private readonly CreatePerformanceReviewValidator _validator;
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePerformanceReviewCommandHandler"/> class.
    /// </summary>
    public CreatePerformanceReviewCommandHandler(
        IPerformanceReviewRepository repository,
        IEmployeeServiceClient employeeService,
        INotificationServiceClient notificationService,
        CreatePerformanceReviewValidator validator,
        IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _employeeService = employeeService;
        _notificationService = notificationService;
        _validator = validator;
        _publishEndpoint = publishEndpoint;
    }

    /// <summary>
    /// Executes the creation of a performance review cycle.
    /// </summary>
    /// <param name="command">The command details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created review or an error message.</returns>
    public async Task<(PerformanceReview? Review, string? Error)> HandleAsync(CreatePerformanceReviewCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return (null, validationResult.Error);
        }

        if (!await _employeeService.ValidateEmployeeExistsAsync(command.EmployeeId, cancellationToken))
        {
            return (null, "Employee not found.");
        }

        // Data Volume Limits
        var count = await _repository.CountByEmployeeIdAsync(command.EmployeeId, cancellationToken);
        if (count >= 50)
        {
            return (null, "DATA_VOLUME_LIMIT_REACHED: Maximum of 50 reviews per employee.");
        }
        if (count >= 40)
        {
            await _notificationService.SendDataVolumeWarningAsync(command.EmployeeId, "PerformanceReview", count, 50, cancellationToken);
        }

        var review = new PerformanceReview
        {
            Id = Guid.NewGuid(),
            EmployeeId = command.EmployeeId,
            ReviewerId = Guid.Empty, // Will be set or handled based on logic
            ReviewCycle = command.ReviewCycle,
            ReviewPeriodStart = command.ReviewPeriodStart,
            ReviewPeriodEnd = command.ReviewPeriodEnd,
            SelfAssessment = command.SelfAssessment,
            Status = ReviewStatus.Draft,
            CreatedDate = DateTime.UtcNow
        };

        var createdReview = await _repository.CreateAsync(review, cancellationToken);

        await _publishEndpoint.Publish(new Maliev.MessagingContracts.Generated.PerformanceReviewCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "PerformanceReviewCreatedEvent",
            MessageType: Maliev.MessagingContracts.Generated.MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "PerformanceService",
            ConsumedBy: ["NotificationService", "AnalyticsService"],
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new Maliev.MessagingContracts.Generated.PerformanceReviewCreatedEventPayload(
                ReviewId: createdReview.Id,
                EmployeeId: createdReview.EmployeeId,
                ReviewerId: createdReview.ReviewerId,
                ReviewCycle: createdReview.ReviewCycle.ToString(),
                ReviewPeriodStart: new DateTimeOffset(createdReview.ReviewPeriodStart, TimeSpan.Zero),
                ReviewPeriodEnd: new DateTimeOffset(createdReview.ReviewPeriodEnd, TimeSpan.Zero)
            )
        ), cancellationToken);

        return (createdReview, null);
    }
}