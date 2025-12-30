# Research & Architectural Decisions: Performance Management Service

**Date**: 2025-12-28
**Feature**: Performance Management Service
**Branch**: 001-performance-service

## Overview

This document captures architectural decisions, technology selections, and best practices research for the Performance Management Service implementation. All decisions align with the MALIEV Microservices Constitution and leverage the established tech stack (.NET 10, PostgreSQL, RabbitMQ, Redis).

---

## 1. Data Storage Strategy

### Decision: PostgreSQL 18 with EF Core 10.x

**Rationale**:
- PostgreSQL provides ACID guarantees essential for performance review data integrity
- JSON/JSONB column support for flexible fields like `progress_updates` and `check_in_notes`
- Full-text search capabilities for searching review assessments and feedback
- Mature EF Core support with migration tooling
- Encryption at rest available through PostgreSQL Transparent Data Encryption (TDE) or disk encryption

**Alternatives Considered**:
- **MongoDB**: Rejected - NoSQL not suitable for relational data (reviews → goals → feedback)
- **SQL Server**: Rejected - PostgreSQL is standard for MALIEV microservices
- **In-memory database**: Rejected - Constitution prohibits in-memory substitutes for testing/production

**Implementation Notes**:
- Use EF Core entity configurations for schema definition
- Implement soft delete for compliance (mark deleted, don't remove data)
- Partition large tables by `created_date` if volume exceeds 10M records
- Use `UUID` primary keys for distributed ID generation

---

## 2. Field-Level Encryption for PII

### Decision: Application-layer encryption using .NET Data Protection API

**Rationale**:
- FR-067 requires field-level encryption for PII (employee names, employee numbers, reviewer identifiers)
- .NET Data Protection API provides built-in key management, rotation, and encryption
- Encryption occurs before data reaches PostgreSQL, protecting against database compromises
- Supports key storage in Google Secret Manager or Azure Key Vault

**Alternatives Considered**:
- **PostgreSQL pgcrypto extension**: Rejected - Encryption in database exposes keys to DBAs
- **Transparent Data Encryption (TDE)**: Rejected - Encrypts entire database, not field-level
- **Custom encryption**: Rejected - .NET Data Protection API is battle-tested and supports key rotation

**Implementation Pattern**:
```csharp
public class PerformanceReview
{
    public Guid Id { get; set; }

    [ProtectedPersonalData]
    public string EmployeeName { get; set; } // Encrypted before persistence

    [ProtectedPersonalData]
    public string EmployeeNumber { get; set; } // Encrypted before persistence

    public string SelfAssessment { get; set; } // Not PII, stored plaintext
}
```

**Key Management**:
- Store encryption keys in Google Secret Manager
- Implement automatic key rotation every 90 days
- Maintain key version history for decrypting historical data

---

## 3. Circuit Breaker Pattern for External Services

### Decision: Polly with Maliev.Aspire.ServiceDefaults resilience handlers

**Rationale**:
- FR-069 requires circuit breakers for all external service calls
- Maliev.Aspire.ServiceDefaults provides `AddStandardResilienceHandler()` extension
- Polly is industry-standard for .NET resilience patterns
- Pre-configured with exponential backoff, circuit breaker, and timeout policies

**Alternatives Considered**:
- **Custom retry logic**: Rejected - Reinventing the wheel, lacks circuit breaker state machine
- **Steeltoe**: Rejected - Polly is more mature and well-integrated with .NET

**Configuration** (from ServiceDefaults):
```csharp
builder.Services.AddHttpClient("EmployeeService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:EmployeeService:BaseUrl"]!);
}).AddStandardResilienceHandler(); // Includes circuit breaker, retry, timeout

builder.Services.AddHttpClient("NotificationService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ExternalServices:NotificationService:BaseUrl"]!);
}).AddStandardResilienceHandler();
```

**Circuit Breaker Behavior**:
- Opens after 5 consecutive failures
- Half-open after 30 seconds (allows one test request)
- Closes if test request succeeds
- Retry: Maximum 3 attempts with exponential backoff (2s, 4s, 8s)
- Timeout: 10 seconds per request

---

## 4. Background Services for Scheduled Reminders

### Decision: IHostedService with Cron expression scheduling

**Rationale**:
- ASP.NET Core built-in `IHostedService` for background tasks
- NCronTab or Cronos library for cron expression parsing
- No external scheduler dependency (no Hangfire, no Quartz.NET)
- Aligns with Constitution principle of simplicity

**Alternatives Considered**:
- **Hangfire**: Rejected - Adds complexity, requires separate dashboard, overkill for simple reminders
- **Quartz.NET**: Rejected - Heavy-weight scheduler, unnecessary for two simple cron jobs
- **Azure Functions/AWS Lambda**: Rejected - Adds external dependency, violates microservice autonomy

**Implementation Pattern**:
```csharp
public class PerformanceReviewReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CronExpression _cron = CronExpression.Parse("0 8 * * *"); // Daily at 8 AM

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var next = _cron.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            var delay = next - DateTimeOffset.Now;
            await Task.Delay(delay, stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPerformanceReviewRepository>();
            // Send reminders logic
        }
    }
}
```

**Cron Schedules**:
- **PerformanceReviewReminderBackgroundService**: `0 8 * * *` (Daily at 8 AM)
- **PIPCheckInReminderBackgroundService**: `0 9 * * 1` (Weekly on Monday at 9 AM)

---

## 5. Data Volume Limits Enforcement

### Decision: Pre-insert validation with soft warnings at 80%

**Rationale**:
- FR-074, FR-075, FR-076 require limits: 50 reviews, 100 goals, 200 feedback per employee
- Prevent unbounded database growth and performance degradation
- Soft warnings at 80% (40 reviews, 80 goals, 160 feedback) alert HR proactively
- Hard limits block inserts, forcing archival

**Alternatives Considered**:
- **No limits**: Rejected - Unbounded growth causes query performance issues
- **Time-based archival only**: Rejected - Doesn't prevent outlier employees with excessive data
- **Database triggers**: Rejected - Business logic belongs in application layer

**Implementation Pattern**:
```csharp
public async Task<Result<PerformanceReview>> CreateAsync(PerformanceReview review, CancellationToken ct)
{
    var count = await _context.PerformanceReviews
        .CountAsync(r => r.EmployeeId == review.EmployeeId, ct);

    if (count >= 50)
        return Result.Failure("DATA_VOLUME_LIMIT_REACHED: Employee has reached maximum 50 reviews. Archive older reviews before creating new ones.");

    if (count >= 40)
        await _notificationService.SendWarningAsync(review.EmployeeId, "Approaching review limit (40/50)");

    _context.PerformanceReviews.Add(review);
    await _context.SaveChangesAsync(ct);
    return Result.Success(review);
}
```

---

## 6. Archival to Cold Storage After 7 Years

### Decision: Scheduled archival job moving data to Google Cloud Storage

**Rationale**:
- FR-059 requires archival after 7 years with manual HR review before purge
- Google Cloud Storage provides cost-effective cold storage
- Data exported as CSV/Parquet for long-term retention
- Manual review step prevents accidental deletion during legal holds

**Alternatives Considered**:
- **Soft delete in PostgreSQL**: Rejected - Still consumes database storage and impacts performance
- **Archive table in same database**: Rejected - Doesn't reduce database size
- **Immediate hard delete**: Rejected - No recovery option, violates legal hold requirements

**Archival Workflow**:
1. Monthly background job queries reviews older than 7 years
2. Export to CSV/Parquet with encryption
3. Upload to Google Cloud Storage bucket with lifecycle policy (Nearline → Archive classes)
4. Mark records as `archived` in PostgreSQL (soft delete, not hard delete)
5. HR dashboard lists archived reviews pending manual purge approval
6. After HR approval + legal hold verification, hard delete from PostgreSQL

**Storage Cost Optimization**:
- Use Parquet format for 60% compression vs CSV
- Google Cloud Storage Archive class: $0.0012/GB/month
- Estimated 10GB/year → $0.14/year for archived data

---

## 7. Observability Stack Integration

### Decision: OpenTelemetry with Maliev.Aspire.ServiceDefaults

**Rationale**:
- FR-060, FR-061, FR-062 require structured logging, metrics, distributed tracing
- Maliev.Aspire.ServiceDefaults provides pre-configured OpenTelemetry setup
- Structured logging via Serilog with JSON output
- Metrics via OpenTelemetry .NET SDK (Prometheus-compatible)
- Distributed tracing via OpenTelemetry with W3C Trace Context propagation

**Metrics to Track**:
- **Request latency**: `http.server.request.duration` (histogram, P50, P95, P99)
- **Throughput**: `http.server.request.count` (counter)
- **Error rate**: `http.server.request.errors` (counter, by status code)
- **Review completion rate**: `performance.reviews.completed` (counter, by rating)
- **Goal completion rate**: `performance.goals.completed` (counter, by status)
- **PIP outcomes**: `performance.pips.outcomes` (counter, by outcome type)
- **Notification delivery success**: `performance.notifications.sent` (counter, success/failure)

**Logging Best Practices**:
- Use structured logging with `ILogger<T>`
- Include correlation ID in all log entries (`TraceId` from Activity)
- Log security events (authorization failures) at `Warning` level
- Never log PII in plaintext (use hashed employee IDs)

**Distributed Tracing**:
- Automatically propagate `traceparent` header to Employee Service, Notification Service
- Instrument database queries with OpenTelemetry EF Core integration
- Instrument RabbitMQ message publishing/consumption with MassTransit OpenTelemetry support

---

## 8. Event Schema Versioning

### Decision: Semantic versioning with MassTransit message contracts

**Rationale**:
- Explicit Contracts principle requires versioned data contracts
- MassTransit supports message namespaces for versioning: `urn:message:Maliev.Performance:PerformanceReviewCreatedEvent:v1`
- Backward-compatible changes (adding optional fields) increment minor version
- Breaking changes (removing fields, changing types) increment major version

**Alternatives Considered**:
- **No versioning**: Rejected - Breaks consumers when events change
- **Dual publishing**: Rejected - Complexity of publishing v1 and v2 simultaneously

**Versioning Strategy**:
```csharp
// v1 event
public record PerformanceReviewCreatedEventV1(
    Guid ReviewId,
    Guid EmployeeId,
    Guid ReviewerId,
    ReviewCycle ReviewCycle,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd);

// v2 event (adds optional CreatedBy field - backward compatible)
public record PerformanceReviewCreatedEventV2(
    Guid ReviewId,
    Guid EmployeeId,
    Guid ReviewerId,
    ReviewCycle ReviewCycle,
    DateTime ReviewPeriodStart,
    DateTime ReviewPeriodEnd,
    Guid? CreatedBy = null); // Optional, defaults to null
```

**MassTransit Configuration**:
```csharp
cfg.Message<PerformanceReviewCreatedEventV1>(m =>
{
    m.SetEntityName("performance.review.created.v1");
});
```

---

## 9. Pagination Strategy for List Endpoints

### Decision: Cursor-based pagination with `CreatedDate` + `Id` composite key

**Rationale**:
- Performance requirement: Review operations complete in <2 seconds
- Offset-based pagination (`OFFSET 1000 LIMIT 50`) degrades with large offsets
- Cursor-based pagination maintains constant performance regardless of page depth
- Prevents inconsistent results when data changes between page requests

**Alternatives Considered**:
- **Offset-based pagination**: Rejected - Slow for large datasets, inconsistent with concurrent writes
- **Keyset pagination**: Rejected - Requires ordering by unique column (CreatedDate + Id composite is better)

**Implementation Pattern**:
```csharp
public async Task<PagedResult<PerformanceReviewDto>> GetReviewsAsync(
    Guid employeeId,
    string? cursor = null,
    int pageSize = 20,
    CancellationToken ct = default)
{
    var query = _context.PerformanceReviews
        .Where(r => r.EmployeeId == employeeId)
        .OrderByDescending(r => r.CreatedDate)
        .ThenByDescending(r => r.Id);

    if (!string.IsNullOrEmpty(cursor))
    {
        var (createdDate, id) = DecodeCursor(cursor);
        query = query.Where(r => r.CreatedDate < createdDate ||
                                 (r.CreatedDate == createdDate && r.Id.CompareTo(id) < 0));
    }

    var reviews = await query.Take(pageSize + 1).ToListAsync(ct);
    var hasNextPage = reviews.Count > pageSize;
    var items = reviews.Take(pageSize).ToList();

    string? nextCursor = null;
    if (hasNextPage)
    {
        var lastItem = items.Last();
        nextCursor = EncodeCursor(lastItem.CreatedDate, lastItem.Id);
    }

    return new PagedResult<PerformanceReviewDto>(items.Select(Map), nextCursor, hasNextPage);
}
```

**Cursor Encoding**:
- Base64 encode `CreatedDate|Id` (e.g., `2024-12-28T10:00:00Z|123e4567-e89b-12d3-a456-426614174000`)
- Example cursor: `MjAyNC0xMi0yOFQxMDowMDowMFp8MTIzZTQ1NjctZTg5Yi0xMmQzLWE0NTYtNDI2NjE0MTc0MDAw`

---

## 10. Goal Progress Updates Storage

### Decision: Append-only text field with timestamp prefixes

**Rationale**:
- Progress updates are append-only (never edited/deleted)
- PostgreSQL TEXT column supports up to 1GB (far exceeds needs)
- Simpler than separate `goal_progress_updates` table
- Searchable via PostgreSQL full-text search

**Alternatives Considered**:
- **Separate table**: Rejected - Overkill for append-only text, adds join complexity
- **JSONB array**: Rejected - Less human-readable, harder to search
- **Binary large object (BLOB)**: Rejected - Not searchable, not human-readable

**Format**:
```text
[2024-12-20T14:30:00Z] Completed 5 of 10 practice exams for AWS certification
[2024-12-15T09:00:00Z] Scheduled exam date for June 15, 2025
[2024-12-01T11:45:00Z] Enrolled in AWS Solutions Architect course
```

**Append Logic**:
```csharp
public async Task AddProgressUpdateAsync(Guid goalId, string update, CancellationToken ct)
{
    var goal = await _context.Goals.FindAsync(goalId, ct);
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    var entry = $"[{timestamp}] {update}\n";

    goal.ProgressUpdates = string.IsNullOrEmpty(goal.ProgressUpdates)
        ? entry
        : goal.ProgressUpdates + entry;

    goal.ModifiedDate = DateTime.UtcNow;
    await _context.SaveChangesAsync(ct);
}
```

---

## 11. Anonymous Feedback Privacy Enforcement

### Decision: Store `provider_id` with one-way hash, suppress if anonymity compromised

**Rationale**:
- FR-020 requires anonymous feedback to hide provider identity
- Edge case: If only 1 peer provides feedback, anonymity cannot be guaranteed
- Store hashed `provider_id` for audit trails (verify no duplicate feedback)
- Suppress feedback display if `FeedbackType` has only 1 provider

**Alternatives Considered**:
- **Null provider_id**: Rejected - Prevents detecting duplicate submissions
- **Display anyway**: Rejected - Violates anonymity guarantee

**Implementation**:
```csharp
public async Task<List<FeedbackDto>> GetFeedbackAsync(Guid reviewId, CancellationToken ct)
{
    var feedback = await _context.ReviewFeedback
        .Where(f => f.PerformanceReviewId == reviewId)
        .GroupBy(f => f.FeedbackType)
        .ToListAsync(ct);

    var result = new List<FeedbackDto>();
    foreach (var group in feedback)
    {
        if (group.Count() == 1 && group.First().IsAnonymous)
        {
            // Suppress to protect anonymity
            continue;
        }

        foreach (var f in group)
        {
            result.Add(new FeedbackDto
            {
                FeedbackType = f.FeedbackType,
                Feedback = f.Feedback,
                ProviderId = f.IsAnonymous ? null : f.ProviderId, // Mask if anonymous
                SubmittedDate = f.SubmittedDate
            });
        }
    }

    return result;
}
```

---

## 12. Review Period Overlap Validation

### Decision: Unique index on `(employee_id, review_cycle, review_period_start, review_period_end)`

**Rationale**:
- FR-002 requires preventing overlapping review periods for same employee + cycle
- Database unique constraint prevents race conditions (two simultaneous requests)
- Application-level validation provides user-friendly error messages

**Alternatives Considered**:
- **Application-only validation**: Rejected - Race conditions possible with concurrent requests
- **Pessimistic locking**: Rejected - Overkill, unique index is simpler and faster

**Database Constraint**:
```sql
CREATE UNIQUE INDEX idx_perf_reviews_no_overlap
ON performance_reviews(employee_id, review_cycle, review_period_start, review_period_end)
WHERE status != 5; -- Exclude completed reviews from uniqueness check
```

**Application Validation**:
```csharp
public async Task<Result<PerformanceReview>> CreateAsync(PerformanceReview review, CancellationToken ct)
{
    var overlaps = await _context.PerformanceReviews
        .AnyAsync(r => r.EmployeeId == review.EmployeeId &&
                       r.ReviewCycle == review.ReviewCycle &&
                       r.ReviewPeriodStart < review.ReviewPeriodEnd &&
                       r.ReviewPeriodEnd > review.ReviewPeriodStart &&
                       r.Status != ReviewStatus.Completed, ct);

    if (overlaps)
        return Result.Failure("REVIEW_PERIOD_OVERLAP: A review for this cycle already exists in this period.");

    _context.PerformanceReviews.Add(review);
    await _context.SaveChangesAsync(ct);
    return Result.Success(review);
}
```

---

## Summary

All architectural decisions prioritize:
1. **Simplicity**: Leverage built-in .NET/EF Core features over external libraries
2. **Constitution Compliance**: No violations, all choices align with MALIEV standards
3. **Performance**: Cursor-based pagination, query optimization, circuit breakers for resilience
4. **Security**: Field-level encryption, anonymity protection, secure key management
5. **Observability**: Structured logging, distributed tracing, business metrics

**No unresolved technical questions remain.** All clarifications from the spec have been addressed.
