using Maliev.MessagingContracts.Contracts.Performance;
using Maliev.MessagingContracts;
using Maliev.PerformanceService.Domain.Enums;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Domain;

public class DomainEventTests
{
    [Fact]
    public void PerformanceReviewCreatedEvent_Test()
    {
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddYears(1);

        var @event = new PerformanceReviewCreatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PerformanceReviewCreatedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PerformanceReviewCreatedEventPayload(
                ReviewId: reviewId,
                EmployeeId: employeeId,
                ReviewerId: reviewerId,
                ReviewCycle: "Annual",
                ReviewPeriodStart: start,
                ReviewPeriodEnd: end
            )
        );

        Assert.Equal(reviewId, @event.Payload.ReviewId);
        Assert.Equal(employeeId, @event.Payload.EmployeeId);
        Assert.Equal(reviewerId, @event.Payload.ReviewerId);
    }

    [Fact]
    public void PerformanceReviewSubmittedEvent_Test()
    {
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;

        var @event = new PerformanceReviewSubmittedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PerformanceReviewSubmittedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PerformanceReviewSubmittedEventPayload(
                ReviewId: reviewId,
                EmployeeId: employeeId,
                OverallRating: 4,
                SubmittedDate: date
            )
        );

        Assert.Equal(reviewId, @event.Payload.ReviewId);
        Assert.Equal(4, @event.Payload.OverallRating);
    }

    [Fact]
    public void PerformanceGoalCompletedEvent_Test()
    {
        var goalId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;

        var @event = new PerformanceGoalCompletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PerformanceGoalCompletedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PerformanceGoalCompletedEventPayload(
                GoalId: goalId,
                EmployeeId: employeeId,
                Description: "Goal Description",
                CompletedDate: date
            )
        );

        Assert.Equal(goalId, @event.Payload.GoalId);
        Assert.Equal("Goal Description", @event.Payload.Description);
    }

    [Fact]
    public void PerformancePIPInitiatedEvent_Test()
    {
        var pipId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddMonths(3);

        var @event = new PerformancePIPInitiatedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PerformancePIPInitiatedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PerformancePIPInitiatedEventPayload(
                PipId: pipId,
                EmployeeId: employeeId,
                StartDate: start,
                EndDate: end,
                Reason: "Reason"
            )
        );

        Assert.Equal(pipId, @event.Payload.PipId);
        Assert.Equal(employeeId, @event.Payload.EmployeeId);
    }

    [Fact]
    public void PIPCompletedEvent_Test()
    {
        var pipId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTimeOffset.UtcNow;

        var @event = new PIPCompletedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: nameof(PIPCompletedEvent),
            MessageType: MessageType.Event,
            MessageVersion: "1.0.0",
            PublishedBy: "Test",
            ConsumedBy: Array.Empty<string>(),
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: false,
            Payload: new PIPCompletedEventPayload(
                PipId: pipId,
                EmployeeId: employeeId,
                Outcome: "Successful",
                CompletedDate: date
            )
        );

        Assert.Equal(pipId, @event.Payload.PipId);
        Assert.Equal("Successful", @event.Payload.Outcome);
    }
}
