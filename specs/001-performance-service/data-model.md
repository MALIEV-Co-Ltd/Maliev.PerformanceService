# Data Model: Performance Management Service

**Date**: 2025-12-28
**Feature**: Performance Management Service
**Branch**: 001-performance-service

## Overview

This document defines the complete data model for the Performance Management Service, including entities, relationships, validation rules, state machines, and database schema. The model supports performance reviews, goals, PIPs, and 360-degree feedback with comprehensive audit trails and data volume enforcement.

---

## Entity Relationship Diagram

```
┌─────────────────────────┐
│   PerformanceReview     │
├─────────────────────────┤
│ Id (PK)                 │
│ EmployeeId (FK)         │──┐
│ ReviewerId (FK)         │  │
│ ReviewCycle             │  │
│ ReviewPeriodStart       │  │
│ ReviewPeriodEnd         │  │
│ SelfAssessment (TEXT)   │  │
│ ManagerAssessment       │  │
│ OverallRating           │  │
│ Status                  │  │
│ SubmittedDate           │  │
│ AcknowledgedDate        │  │
│ CreatedDate             │  │
│ ModifiedDate            │  │
└─────────────────────────┘  │
           │                 │
           │ 1:N             │
           ▼                 │
┌─────────────────────────┐  │
│     ReviewFeedback      │  │
├─────────────────────────┤  │
│ Id (PK)                 │  │
│ PerformanceReviewId(FK) │──┘
│ ProviderId (hashed)     │
│ FeedbackType            │
│ Feedback (TEXT)         │
│ IsAnonymous             │
│ SubmittedDate           │
└─────────────────────────┘


┌─────────────────────────┐
│         Goal            │
├─────────────────────────┤
│ Id (PK)                 │
│ EmployeeId (FK)         │
│ PerformanceReviewId(FK) │──┐ (optional link to review)
│ Description (TEXT)      │  │
│ SuccessCriteria (TEXT)  │  │
│ TargetDate              │  │
│ CompletionStatus        │  │
│ ProgressUpdates (TEXT)  │  │ (append-only timestamps)
│ CompletedDate           │  │
│ CreatedDate             │  │
│ ModifiedDate            │  │
└─────────────────────────┘  │
                             │
                             │
┌─────────────────────────┐  │
│ PerformanceReview       │◄─┘
│ (1:N Goals)             │
└─────────────────────────┘


┌────────────────────────────────┐
│  PerformanceImprovementPlan    │
├────────────────────────────────┤
│ Id (PK)                        │
│ EmployeeId (FK)                │
│ InitiatedBy (FK)               │
│ StartDate                      │
│ EndDate                        │
│ Reason (TEXT)                  │
│ ImprovementAreas (TEXT)        │
│ SuccessCriteria (TEXT)         │
│ CheckInNotes (TEXT, append)    │
│ Status                         │
│ Outcome                        │
│ CreatedDate                    │
│ ModifiedDate                   │
└────────────────────────────────┘
```

---

## Entity Definitions

### 1. PerformanceReview

**Purpose**: Represents a formal performance evaluation for an employee during a specific review period.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| `EmployeeId` | UUID | NOT NULL, INDEXED | Employee being reviewed (external reference to Employee Service) |
| `ReviewerId` | UUID | NOT NULL, INDEXED | Manager conducting the review (external reference to Employee Service) |
| `ReviewCycle` | INTEGER | NOT NULL | Enum: 0=Annual, 1=SemiAnnual, 2=Quarterly, 3=Probation |
| `ReviewPeriodStart` | TIMESTAMP WITH TIME ZONE | NOT NULL | Start date of review period |
| `ReviewPeriodEnd` | TIMESTAMP WITH TIME ZONE | NOT NULL | End date of review period |
| `SelfAssessment` | TEXT | NULLABLE | Employee's self-assessment narrative |
| `ManagerAssessment` | TEXT | NULLABLE | Manager's assessment narrative |
| `OverallRating` | INTEGER | NULLABLE | Enum: 1=Unsatisfactory, 2=BelowExpectations, 3=NeedsImprovement, 4=MeetsExpectations, 5=ExceedsExpectations |
| `Status` | INTEGER | NOT NULL, DEFAULT 0, INDEXED | Enum: 0=Draft, 1=SelfAssessmentPending, 2=ManagerReviewPending, 3=Submitted, 4=Acknowledged, 5=Completed |
| `SubmittedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | When manager submitted review for acknowledgment |
| `AcknowledgedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | When employee acknowledged review |
| `CreatedDate` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Audit: When review was created |
| `ModifiedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | Audit: Last modification timestamp |

**Validation Rules**:
- `ReviewPeriodStart` < `ReviewPeriodEnd`
- `SubmittedDate` must be >= `CreatedDate` (if set)
- `AcknowledgedDate` must be >= `SubmittedDate` (if set)
- Unique constraint: No overlapping `(EmployeeId, ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd)` for non-completed reviews
- Data volume limit: Maximum 50 reviews per `EmployeeId` (enforced in application layer with soft warning at 40)

**State Machine** (see section below)

**Indexes**:
- `idx_perf_reviews_employee` ON `(employee_id)` for employee-specific queries
- `idx_perf_reviews_reviewer` ON `(reviewer_id)` for manager dashboards
- `idx_perf_reviews_status` ON `(status)` for filtering pending reviews
- `idx_perf_reviews_no_overlap` UNIQUE ON `(employee_id, review_cycle, review_period_start, review_period_end)` WHERE `status != 5`

---

### 2. Goal

**Purpose**: Represents an objective or target set for an employee, optionally linked to a performance review period.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| `EmployeeId` | UUID | NOT NULL, INDEXED | Employee who owns the goal |
| `PerformanceReviewId` | UUID | NULLABLE, FOREIGN KEY (PerformanceReview.Id) | Optional link to review |
| `Description` | TEXT | NOT NULL | Goal description |
| `SuccessCriteria` | TEXT | NULLABLE | Measurable success criteria (SMART goal) |
| `TargetDate` | TIMESTAMP WITH TIME ZONE | NOT NULL | Target completion date |
| `CompletionStatus` | INTEGER | NOT NULL, DEFAULT 0, INDEXED | Enum: 0=NotStarted, 1=InProgress, 2=AtRisk, 3=Completed, 4=Deferred, 5=Cancelled |
| `ProgressUpdates` | TEXT | NULLABLE | Append-only progress notes with timestamps (format: `[YYYY-MM-DDTHH:MM:SSZ] note\n`) |
| `CompletedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | Actual completion date (set when status = Completed) |
| `CreatedDate` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Audit: When goal was created |
| `ModifiedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | Audit: Last modification timestamp |

**Validation Rules**:
- `TargetDate` must be in the future (when created)
- If `CompletionStatus` = 3 (Completed), `CompletedDate` must be set
- `CompletedDate` must be <= current date (can't complete in future)
- Data volume limit: Maximum 100 goals per `EmployeeId` (enforced in application layer with soft warning at 80)

**State Transitions**:
- NotStarted → InProgress
- InProgress → {AtRisk, Completed, Deferred, Cancelled}
- AtRisk → {Completed, Deferred, Cancelled}
- Completed → InProgress (reopened)
- Deferred → {InProgress, Cancelled}

**Indexes**:
- `idx_goals_employee` ON `(employee_id)` for employee goal lists
- `idx_goals_review` ON `(performance_review_id)` for review-linked goals
- `idx_goals_status` ON `(completion_status)` for filtering by status

---

### 3. PerformanceImprovementPlan

**Purpose**: Represents a formal improvement plan for an employee not meeting performance expectations.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| `EmployeeId` | UUID | NOT NULL, INDEXED | Employee on the PIP |
| `InitiatedBy` | UUID | NOT NULL | HR admin or manager who initiated PIP |
| `StartDate` | TIMESTAMP WITH TIME ZONE | NOT NULL | PIP start date |
| `EndDate` | TIMESTAMP WITH TIME ZONE | NOT NULL | PIP end date (30-90 days typical) |
| `Reason` | TEXT | NOT NULL | Reason for initiating PIP |
| `ImprovementAreas` | TEXT | NOT NULL | Areas requiring improvement |
| `SuccessCriteria` | TEXT | NOT NULL | Measurable success criteria for PIP completion |
| `CheckInNotes` | TEXT | NULLABLE | Append-only check-in notes with timestamps (format: `[YYYY-MM-DDTHH:MM:SSZ] note\n`) |
| `Status` | INTEGER | NOT NULL, DEFAULT 0, INDEXED | Enum: 0=Active, 1=Extended, 2=Completed, 3=Terminated |
| `Outcome` | INTEGER | NULLABLE | Enum: 0=Successful, 1=Unsuccessful, 2=ExtendedAgain (set when Status = Completed or Terminated) |
| `CreatedDate` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | Audit: When PIP was created |
| `ModifiedDate` | TIMESTAMP WITH TIME ZONE | NULLABLE | Audit: Last modification timestamp |

**Validation Rules**:
- `StartDate` < `EndDate`
- `EndDate` - `StartDate` should be between 30 and 90 days (soft validation, warning only)
- Only ONE active PIP per `EmployeeId` (enforced: `Status IN (0, 1)` must be unique per employee)
- Maximum ONE extension allowed (cannot transition Active → Extended → Extended)
- If `Status` IN (2, 3), `Outcome` must be set

**State Machine**:
- Active → {Extended, Completed, Terminated}
- Extended → {Completed, Terminated}
- Completed, Terminated: Terminal states

**Indexes**:
- `idx_pips_employee` ON `(employee_id)` for employee PIP history
- `idx_pips_status` ON `(status)` for filtering active/completed PIPs
- `idx_pips_active_unique` UNIQUE ON `(employee_id)` WHERE `status IN (0, 1)` (prevents duplicate active PIPs)

---

### 4. ReviewFeedback

**Purpose**: Represents feedback submitted by a stakeholder about an employee's performance during a review.

**Fields**:

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `Id` | UUID | PRIMARY KEY, NOT NULL | Unique identifier |
| `PerformanceReviewId` | UUID | NOT NULL, FOREIGN KEY (PerformanceReview.Id) ON DELETE CASCADE, INDEXED | Review this feedback belongs to |
| `ProviderId` | UUID | NOT NULL | Person who provided feedback (hashed if anonymous) |
| `FeedbackType` | INTEGER | NOT NULL | Enum: 0=Manager, 1=Peer, 2=DirectReport, 3=Self |
| `Feedback` | TEXT | NOT NULL | Feedback narrative |
| `IsAnonymous` | BOOLEAN | NOT NULL, DEFAULT FALSE | Whether feedback is anonymous |
| `SubmittedDate` | TIMESTAMP WITH TIME ZONE | NOT NULL, DEFAULT NOW() | When feedback was submitted |

**Validation Rules**:
- Data volume limit: Maximum 200 feedback entries per `EmployeeId` (aggregate across all reviews, enforced in application layer with soft warning at 160)
- Anonymity protection: If `IsAnonymous` = TRUE and only 1 feedback of that `FeedbackType` exists, suppress feedback display to user

**Anonymity Implementation**:
- `ProviderId` is stored as SHA256 hash if `IsAnonymous` = TRUE
- Hash prevents duplicate submissions but doesn't reveal identity
- Application layer checks feedback count by `FeedbackType` before displaying anonymous feedback

**Indexes**:
- `idx_feedback_review` ON `(performance_review_id)` for fetching feedback by review

---

## Enumerations

### ReviewCycle

| Value | Name | Description |
|-------|------|-------------|
| 0 | Annual | Yearly performance review |
| 1 | SemiAnnual | Twice-yearly review |
| 2 | Quarterly | Every 3 months |
| 3 | Probation | Probationary period review (new hires) |

### PerformanceRating

| Value | Name | Description |
|-------|------|-------------|
| 5 | ExceedsExpectations | Consistently exceeds performance expectations |
| 4 | MeetsExpectations | Consistently meets all performance expectations |
| 3 | NeedsImprovement | Meets some expectations but needs improvement |
| 2 | BelowExpectations | Performance below acceptable standards |
| 1 | Unsatisfactory | Significant performance deficiencies |

### ReviewStatus

| Value | Name | Description |
|-------|------|-------------|
| 0 | Draft | Review created but not started |
| 1 | SelfAssessmentPending | Waiting for employee self-assessment |
| 2 | ManagerReviewPending | Self-assessment complete, waiting for manager review |
| 3 | Submitted | Manager submitted review, waiting for employee acknowledgment |
| 4 | Acknowledged | Employee acknowledged review |
| 5 | Completed | Review fully completed and archived |

**State Machine**:
```
Draft → SelfAssessmentPending → ManagerReviewPending → Submitted → Acknowledged → Completed
```

**Transitions**:
- `Draft → SelfAssessmentPending`: System assigns review to employee
- `SelfAssessmentPending → ManagerReviewPending`: Employee submits self-assessment
- `ManagerReviewPending → Submitted`: Manager completes assessment and submits
- `Submitted → Acknowledged`: Employee acknowledges review
- `Acknowledged → Completed`: System marks complete (or manual admin action)

### GoalStatus

| Value | Name | Description |
|-------|------|-------------|
| 0 | NotStarted | Goal created but work hasn't begun |
| 1 | InProgress | Actively working towards goal |
| 2 | AtRisk | Goal is at risk of not being completed on time |
| 3 | Completed | Goal successfully achieved |
| 4 | Deferred | Goal postponed to future period |
| 5 | Cancelled | Goal cancelled (no longer relevant) |

### PIPStatus

| Value | Name | Description |
|-------|------|-------------|
| 0 | Active | PIP currently in progress |
| 1 | Extended | PIP extended beyond original end date (max 1 extension) |
| 2 | Completed | PIP concluded with documented outcome |
| 3 | Terminated | PIP terminated early (e.g., employee resignation) |

### PIPOutcome

| Value | Name | Description |
|-------|------|-------------|
| 0 | Successful | Employee met PIP success criteria |
| 1 | Unsuccessful | Employee did not meet PIP success criteria |
| 2 | ExtendedAgain | PIP extended again (rare, requires special approval) |

### FeedbackType

| Value | Name | Description |
|-------|------|-------------|
| 0 | Manager | Feedback from direct manager |
| 1 | Peer | Feedback from peer/colleague |
| 2 | DirectReport | Feedback from employee's direct report (upward feedback) |
| 3 | Self | Employee's self-feedback |

---

## Database Schema (PostgreSQL)

```sql
-- performance_reviews table
CREATE TABLE performance_reviews (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    reviewer_id UUID NOT NULL,
    review_cycle INTEGER NOT NULL,
    review_period_start TIMESTAMP WITH TIME ZONE NOT NULL,
    review_period_end TIMESTAMP WITH TIME ZONE NOT NULL,
    self_assessment TEXT,
    manager_assessment TEXT,
    overall_rating INTEGER,
    status INTEGER NOT NULL DEFAULT 0,
    submitted_date TIMESTAMP WITH TIME ZONE,
    acknowledged_date TIMESTAMP WITH TIME ZONE,
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    modified_date TIMESTAMP WITH TIME ZONE,

    CONSTRAINT chk_review_period CHECK (review_period_start < review_period_end),
    CONSTRAINT chk_submitted_after_created CHECK (submitted_date IS NULL OR submitted_date >= created_date),
    CONSTRAINT chk_acknowledged_after_submitted CHECK (acknowledged_date IS NULL OR acknowledged_date >= submitted_date),
    CONSTRAINT chk_review_cycle CHECK (review_cycle BETWEEN 0 AND 3),
    CONSTRAINT chk_overall_rating CHECK (overall_rating IS NULL OR overall_rating BETWEEN 1 AND 5),
    CONSTRAINT chk_status CHECK (status BETWEEN 0 AND 5)
);

CREATE INDEX idx_perf_reviews_employee ON performance_reviews(employee_id);
CREATE INDEX idx_perf_reviews_reviewer ON performance_reviews(reviewer_id);
CREATE INDEX idx_perf_reviews_status ON performance_reviews(status);
CREATE INDEX idx_perf_reviews_created ON performance_reviews(created_date DESC); -- For pagination
CREATE UNIQUE INDEX idx_perf_reviews_no_overlap ON performance_reviews(employee_id, review_cycle, review_period_start, review_period_end) WHERE status != 5;

-- goals table
CREATE TABLE goals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    performance_review_id UUID REFERENCES performance_reviews(id) ON DELETE SET NULL,
    description TEXT NOT NULL,
    success_criteria TEXT,
    target_date TIMESTAMP WITH TIME ZONE NOT NULL,
    completion_status INTEGER NOT NULL DEFAULT 0,
    progress_updates TEXT,
    completed_date TIMESTAMP WITH TIME ZONE,
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    modified_date TIMESTAMP WITH TIME ZONE,

    CONSTRAINT chk_completed_date CHECK (completed_date IS NULL OR completed_date <= NOW()),
    CONSTRAINT chk_completion_status CHECK (completion_status BETWEEN 0 AND 5)
);

CREATE INDEX idx_goals_employee ON goals(employee_id);
CREATE INDEX idx_goals_review ON goals(performance_review_id) WHERE performance_review_id IS NOT NULL;
CREATE INDEX idx_goals_status ON goals(completion_status);
CREATE INDEX idx_goals_created ON goals(created_date DESC); -- For pagination

-- performance_improvement_plans table
CREATE TABLE performance_improvement_plans (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    employee_id UUID NOT NULL,
    initiated_by UUID NOT NULL,
    start_date TIMESTAMP WITH TIME ZONE NOT NULL,
    end_date TIMESTAMP WITH TIME ZONE NOT NULL,
    reason TEXT NOT NULL,
    improvement_areas TEXT NOT NULL,
    success_criteria TEXT NOT NULL,
    check_in_notes TEXT,
    status INTEGER NOT NULL DEFAULT 0,
    outcome INTEGER,
    created_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    modified_date TIMESTAMP WITH TIME ZONE,

    CONSTRAINT chk_pip_dates CHECK (start_date < end_date),
    CONSTRAINT chk_pip_status CHECK (status BETWEEN 0 AND 3),
    CONSTRAINT chk_pip_outcome CHECK (outcome IS NULL OR outcome BETWEEN 0 AND 2),
    CONSTRAINT chk_pip_outcome_required CHECK ((status IN (2, 3) AND outcome IS NOT NULL) OR status IN (0, 1))
);

CREATE INDEX idx_pips_employee ON performance_improvement_plans(employee_id);
CREATE INDEX idx_pips_status ON performance_improvement_plans(status);
CREATE UNIQUE INDEX idx_pips_active_unique ON performance_improvement_plans(employee_id) WHERE status IN (0, 1);

-- review_feedback table
CREATE TABLE review_feedback (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    performance_review_id UUID NOT NULL REFERENCES performance_reviews(id) ON DELETE CASCADE,
    provider_id UUID NOT NULL, -- Hashed if anonymous
    feedback_type INTEGER NOT NULL,
    feedback TEXT NOT NULL,
    is_anonymous BOOLEAN NOT NULL DEFAULT FALSE,
    submitted_date TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_feedback_type CHECK (feedback_type BETWEEN 0 AND 3)
);

CREATE INDEX idx_feedback_review ON review_feedback(performance_review_id);
CREATE INDEX idx_feedback_type ON review_feedback(performance_review_id, feedback_type); -- For anonymity checks
```

---

## EF Core Entity Configurations

Example configuration for PerformanceReview:

```csharp
public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("performance_reviews");

        builder.HasKey(pr => pr.Id);
        builder.Property(pr => pr.Id).HasColumnName("id");

        builder.Property(pr => pr.EmployeeId).HasColumnName("employee_id").IsRequired();
        builder.Property(pr => pr.ReviewerId).HasColumnName("reviewer_id").IsRequired();
        builder.Property(pr => pr.ReviewCycle).HasColumnName("review_cycle").IsRequired();
        builder.Property(pr => pr.ReviewPeriodStart).HasColumnName("review_period_start").IsRequired();
        builder.Property(pr => pr.ReviewPeriodEnd).HasColumnName("review_period_end").IsRequired();
        builder.Property(pr => pr.SelfAssessment).HasColumnName("self_assessment").HasColumnType("text");
        builder.Property(pr => pr.ManagerAssessment).HasColumnName("manager_assessment").HasColumnType("text");
        builder.Property(pr => pr.OverallRating).HasColumnName("overall_rating");
        builder.Property(pr => pr.Status).HasColumnName("status").IsRequired();
        builder.Property(pr => pr.SubmittedDate).HasColumnName("submitted_date");
        builder.Property(pr => pr.AcknowledgedDate).HasColumnName("acknowledged_date");
        builder.Property(pr => pr.CreatedDate).HasColumnName("created_date").HasDefaultValueSql("NOW()").IsRequired();
        builder.Property(pr => pr.ModifiedDate).HasColumnName("modified_date");

        // Indexes
        builder.HasIndex(pr => pr.EmployeeId).HasDatabaseName("idx_perf_reviews_employee");
        builder.HasIndex(pr => pr.ReviewerId).HasDatabaseName("idx_perf_reviews_reviewer");
        builder.HasIndex(pr => pr.Status).HasDatabaseName("idx_perf_reviews_status");
        builder.HasIndex(pr => pr.CreatedDate).HasDatabaseName("idx_perf_reviews_created").IsDescending();

        // Unique constraint for overlap prevention
        builder.HasIndex(pr => new { pr.EmployeeId, pr.ReviewCycle, pr.ReviewPeriodStart, pr.ReviewPeriodEnd })
            .IsUnique()
            .HasDatabaseName("idx_perf_reviews_no_overlap")
            .HasFilter("status != 5");

        // Relationships
        builder.HasMany(pr => pr.Goals)
            .WithOne(g => g.PerformanceReview)
            .HasForeignKey(g => g.PerformanceReviewId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(pr => pr.Feedback)
            .WithOne(f => f.PerformanceReview)
            .HasForeignKey(f => f.PerformanceReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## Data Volume Limits Summary

| Entity | Limit Per Employee | Soft Warning Threshold | Enforcement |
|--------|-------------------|----------------------|-------------|
| PerformanceReview | 50 | 40 (80%) | Application layer pre-insert validation |
| Goal | 100 | 80 (80%) | Application layer pre-insert validation |
| ReviewFeedback | 200 (aggregate) | 160 (80%) | Application layer pre-insert validation |

**Enforcement Strategy**:
1. Pre-insert: Count existing records for employee
2. If count >= limit: Return error with guidance to archive
3. If count >= warning threshold: Send notification to HR, allow insert
4. Metrics: Track limit-approaching events for proactive monitoring

---

## Archival Strategy

**Trigger**: Reviews older than 7 years from `created_date`
**Process**:
1. Monthly background job queries `performance_reviews` WHERE `created_date < NOW() - INTERVAL '7 years'`
2. Export to encrypted Parquet files in Google Cloud Storage
3. Mark records as `archived` (soft delete, not hard delete immediately)
4. HR dashboard lists archived reviews pending manual purge approval
5. After HR approval + legal hold verification, hard delete from PostgreSQL

**Archival Schema** (Parquet):
- Includes all entity data plus relationships (goals, feedback)
- Compressed using Snappy codec (60% reduction)
- Encrypted using Google Cloud KMS
- Stored in Google Cloud Storage Archive class

---

## Summary

The data model provides:
- **Complete lifecycle tracking** for reviews, goals, PIPs, and feedback
- **Audit trails** with creation/modification timestamps on all entities
- **State machines** to enforce valid workflow transitions
- **Data volume limits** to prevent unbounded growth
- **Anonymity protection** for 360-degree feedback
- **Overlap prevention** for review periods
- **Soft deletes** for compliance and legal hold requirements
- **Efficient pagination** with indexed timestamps
- **Foreign key relationships** with appropriate cascade behaviors

All design choices align with the MALIEV Microservices Constitution and support the functional requirements defined in the specification.
