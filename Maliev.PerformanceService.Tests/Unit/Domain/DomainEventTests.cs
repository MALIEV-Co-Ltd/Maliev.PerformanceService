using Maliev.PerformanceService.Domain.Events;
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
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddYears(1);
        var created = DateTime.UtcNow;

        var @event = new PerformanceReviewCreatedEvent(
            reviewId,
            employeeId,
            reviewerId,
            ReviewCycle.Annual,
            start,
            end,
            created);

        Assert.Equal(reviewId, @event.ReviewId);
        Assert.Equal(employeeId, @event.EmployeeId);
        Assert.Equal(reviewerId, @event.ReviewerId);
    }

    [Fact]
    public void PerformanceReviewSubmittedEvent_Test()
    {
        var reviewId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        var @event = new PerformanceReviewSubmittedEvent(
            reviewId,
            employeeId,
            PerformanceRating.ExceedsExpectations,
            date);

        Assert.Equal(reviewId, @event.ReviewId);
        Assert.Equal(PerformanceRating.ExceedsExpectations, @event.OverallRating);
    }

    [Fact]
    public void PerformanceGoalCompletedEvent_Test()
    {
        var goalId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        var @event = new PerformanceGoalCompletedEvent(
            goalId,
            employeeId,
            "Goal Description",
            date);

        Assert.Equal(goalId, @event.GoalId);
        Assert.Equal("Goal Description", @event.Description);
    }

    [Fact]
    public void PerformancePIPInitiatedEvent_Test()
    {
        var pipId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddMonths(3);

        var @event = new PerformancePIPInitiatedEvent(
            pipId,
            employeeId,
            initiatorId,
            start,
            end,
            "Reason");

        Assert.Equal(pipId, @event.PIPId);
        Assert.Equal(initiatorId, @event.InitiatorId);
    }

    [Fact]
    public void PIPCompletedEvent_Test()
    {
        var pipId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        var @event = new PIPCompletedEvent(
            pipId,
            employeeId,
            PIPOutcome.Successful,
            date);

        Assert.Equal(pipId, @event.PIPId);
        Assert.Equal(PIPOutcome.Successful, @event.Outcome);
    }
}
