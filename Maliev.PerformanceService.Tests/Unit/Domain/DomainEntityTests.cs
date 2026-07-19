using Maliev.PerformanceService.Domain.Entities;
using Maliev.PerformanceService.Domain.Enums;
using Xunit;

namespace Maliev.PerformanceService.Tests.Unit.Domain;

public class DomainEntityTests
{
    [Fact]
    public void PerformanceReview_PropertyTest()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddMonths(-6);
        var end = DateTime.UtcNow;
        var created = DateTime.UtcNow;
        var modified = DateTime.UtcNow;

        var review = new PerformanceReview
        {
            Id = id,
            EmployeeId = employeeId,
            ReviewerId = reviewerId,
            ReviewCycle = ReviewCycle.Annual,
            ReviewPeriodStart = start,
            ReviewPeriodEnd = end,
            SelfAssessment = "Self",
            ManagerAssessment = "Manager",
            OverallRating = PerformanceRating.ExceedsExpectations,
            Status = ReviewStatus.Submitted,
            SubmittedDate = end,
            AcknowledgedDate = end.AddDays(1),
            CreatedDate = created,
            ModifiedDate = modified,
            IsArchived = false
        };

        Assert.Equal(id, review.Id);
        Assert.Equal(employeeId, review.EmployeeId);
        Assert.Equal(reviewerId, review.ReviewerId);
        Assert.Equal(ReviewCycle.Annual, review.ReviewCycle);
        Assert.Equal(start, review.ReviewPeriodStart);
        Assert.Equal(end, review.ReviewPeriodEnd);
        Assert.Equal("Self", review.SelfAssessment);
        Assert.Equal("Manager", review.ManagerAssessment);
        Assert.Equal(PerformanceRating.ExceedsExpectations, review.OverallRating);
        Assert.Equal(ReviewStatus.Submitted, review.Status);
        Assert.Equal(end, review.SubmittedDate);
        Assert.Equal(end.AddDays(1), review.AcknowledgedDate);
        Assert.Equal(created, review.CreatedDate);
        Assert.Equal(modified, review.ModifiedDate);
        Assert.False(review.IsArchived);
    }

    [Fact]
    public void PerformanceImprovementPlan_PropertyTest()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var initiatorId = Guid.NewGuid();
        var start = DateTime.UtcNow;
        var end = DateTime.UtcNow.AddMonths(3);

        var pip = new PerformanceImprovementPlan
        {
            Id = id,
            EmployeeId = employeeId,
            InitiatorId = initiatorId,
            StartDate = start,
            EndDate = end,
            Reason = "Reason",
            ImprovementAreas = "Areas",
            SuccessCriteria = "Criteria",
            CheckInNotes = "Notes",
            Status = PIPStatus.Active,
            Outcome = PIPOutcome.Successful,
            ExtensionCount = 1,
            CreatedDate = start,
            ModifiedDate = start
        };

        Assert.Equal(id, pip.Id);
        Assert.Equal(employeeId, pip.EmployeeId);
        Assert.Equal(initiatorId, pip.InitiatorId);
        Assert.Equal(start, pip.StartDate);
        Assert.Equal(end, pip.EndDate);
        Assert.Equal("Reason", pip.Reason);
        Assert.Equal("Areas", pip.ImprovementAreas);
        Assert.Equal("Criteria", pip.SuccessCriteria);
        Assert.Equal("Notes", pip.CheckInNotes);
        Assert.Equal(PIPStatus.Active, pip.Status);
        Assert.Equal(PIPOutcome.Successful, pip.Outcome);
        Assert.Equal(1, pip.ExtensionCount);
    }

    [Fact]
    public void Goal_PropertyTest()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var goal = new Goal
        {
            Id = id,
            EmployeeId = employeeId,
            Description = "Description",
            SuccessCriteria = "Criteria",
            TargetCompletionDate = DateTime.UtcNow.AddMonths(1),
            CompletionDate = DateTime.UtcNow.AddMonths(1),
            CurrentStatus = GoalStatus.Completed,
            ProgressUpdates = "Done",
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        Assert.Equal(id, goal.Id);
        Assert.Equal(employeeId, goal.EmployeeId);
        Assert.Equal("Description", goal.Description);
        Assert.Equal("Criteria", goal.SuccessCriteria);
        Assert.Equal(GoalStatus.Completed, goal.CurrentStatus);
        Assert.Equal("Done", goal.ProgressUpdates);
    }

    [Fact]
    public void ReviewFeedback_PropertyTest()
    {
        var id = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var feedback = new ReviewFeedback
        {
            Id = id,
            PerformanceReviewId = reviewId,
            ProviderId = providerId,
            FeedbackType = FeedbackType.Peer,
            Feedback = "Great work",
            SubmittedDate = DateTime.UtcNow,
            IsAnonymous = true,
            CreatedDate = DateTime.UtcNow
        };

        Assert.Equal(id, feedback.Id);
        Assert.Equal(reviewId, feedback.PerformanceReviewId);
        Assert.Equal(providerId, feedback.ProviderId);
        Assert.Equal(FeedbackType.Peer, feedback.FeedbackType);
        Assert.Equal("Great work", feedback.Feedback);
        Assert.True(feedback.IsAnonymous);
    }

    [Fact]
    public void DisciplinaryAction_PropertyTest()
    {
        var id = Guid.NewGuid();
        var employeeId = Guid.NewGuid();

        var action = new DisciplinaryAction
        {
            Id = id,
            EmployeeId = employeeId,
            InitiatorId = Guid.NewGuid(),
            ActionDate = DateTime.UtcNow,
            ActionType = "Warning",
            Reason = "Late",
            ValidityPeriod = "3 months",
            CreatedDate = DateTime.UtcNow
        };

        Assert.Equal(id, action.Id);
        Assert.Equal("Warning", action.ActionType);
        Assert.Equal("Late", action.Reason);
    }
}
