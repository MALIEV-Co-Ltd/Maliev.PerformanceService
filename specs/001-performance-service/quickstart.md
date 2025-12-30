# Quick Start Guide: Performance Management Service

**Date**: 2025-12-28
**Feature**: Performance Management Service
**Branch**: 001-performance-service

## Overview

This quickstart guide provides the essential information to understand and work with the Performance Management Service. It covers the architecture, key workflows, development setup, and common operations.

---

## Architecture Overview

```
┌─────────────────┐
│  API Gateway    │
│  (Auth/JWT)     │
└────────┬────────┘
         │
         ▼
┌─────────────────────────────────┐
│  Performance Service (ASP.NET)  │
├─────────────────────────────────┤
│  Controllers                    │
│  ├── PerformanceReviewsController│
│  ├── GoalsController            │
│  ├── PIPsController             │
│  └── FeedbackController         │
│                                 │
│  Command/Query Handlers (CQRS) │
│  Application Logic + Validation │
│                                 │
│  Domain Entities & Events       │
└───────┬─────────────────────┬───┘
        │                     │
        ▼                     ▼
┌───────────────┐    ┌───────────────┐
│  PostgreSQL   │    │   RabbitMQ    │
│  (Primary DB) │    │  (Events Bus) │
└───────────────┘    └───────────────┘
        ▲                     │
        │                     │
┌───────────────┐             ▼
│  Redis Cache  │    ┌─────────────────┐
│  (Distributed)│    │ Employee Service│
└───────────────┘    │ Notification Svc│
                     └─────────────────┘
```

**Key Components**:
- **API Layer**: REST endpoints with JWT authentication and permission-based authorization
- **Application Layer**: CQRS command/query handlers, validators
- **Domain Layer**: Entities, enums, events, business rules
- **Infrastructure Layer**: EF Core DbContext, repositories, background services, event consumers, external clients
- **External Dependencies**: Employee Service (employee data), Notification Service (reminders/alerts)

---

## Core Workflows

### 1. Performance Review Lifecycle

```
┌─────────────────┐
│  Create Review  │ (HR/Manager)
│  - Annual/Semi- │
│  - Review Period│
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Employee Self- │ (Employee)
│  Assessment     │
│  - Achievements │
│  - Challenges   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ Manager Review  │ (Manager)
│  - Assessment   │
│  - Rating       │
│  - Submit       │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Employee       │ (Employee)
│  Acknowledgment │
│  - Read & Sign  │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   Completed     │
│   (Archived)    │
└─────────────────┘
```

**States**: Draft → SelfAssessmentPending → ManagerReviewPending → Submitted → Acknowledged → Completed

**Business Rules**:
- Self-assessment required before manager review
- Acknowledgment required within 7 days (reminders sent)
- No overlapping review periods for same employee + cycle

### 2. Goal Setting & Tracking

```
Employee/Manager creates goal → Progress updates added → Status transitions → Goal completed
                                         │
                                         └─> Link to review (optional)
```

**Goal Statuses**: NotStarted → InProgress → {AtRisk, Completed, Deferred, Cancelled}

**Features**:
- Append-only progress notes with timestamps
- Alerts when status = "AtRisk"
- Can be linked to performance review periods

### 3. Performance Improvement Plan (PIP)

```
HR initiates PIP → Weekly check-ins → Record outcome → Complete/Terminate
                        │
                        └─> One extension allowed
```

**PIP Statuses**: Active → {Extended, Completed, Terminated}

**Business Rules**:
- Only ONE active PIP per employee
- Weekly check-ins required (Monday 9 AM reminders)
- Maximum one extension allowed
- Outcome must be recorded (Successful/Unsuccessful)

### 4. 360-Degree Feedback

```
Manager/Peer/DirectReport submits feedback → Aggregated by type → Visible in review
                       │
                       └─> Can be anonymous
```

**Feedback Types**: Manager, Peer, DirectReport, Self

**Anonymity Protection**:
- Provider ID hashed if anonymous
- Feedback suppressed if only 1 provider of that type (preserves anonymity)

---

## Development Setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for Testcontainers)
- PostgreSQL client (optional, for manual DB access)
- Git

### 1. Clone Repository

```bash
git clone https://github.com/MALIEV-Co-Ltd/Maliev.PerformanceService.git
cd Maliev.PerformanceService
git checkout 001-performance-service
```

### 2. Configure Secrets

Create a `.env` file in the repository root:

```env
# NuGet GitHub Packages Authentication
NUGET_USERNAME=your-github-username
NUGET_PASSWORD=your-github-pat-with-read-packages-scope

# Database
CONNECTIONSTRINGS__PERFORMANCEDBCONTEXT=Host=localhost;Port=5432;Database=performance_dev;Username=postgres;Password=yourpassword

# External Services
EXTERNALSERVICES__EMPLOYEESERVICE__BASEURL=https://api.maliev.com/employee/v1
EXTERNALSERVICES__NOTIFICATIONSERVICE__BASEURL=https://api.maliev.com/notification/v1

# RabbitMQ
RABBITMQ__HOST=localhost
RABBITMQ__USERNAME=guest
RABBITMQ__PASSWORD=guest

# Redis
REDIS__CONNECTIONSTRING=localhost:6379
```

### 3. Run Infrastructure (Docker Compose)

```bash
# Start PostgreSQL, RabbitMQ, Redis
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=yourpassword postgres:18
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management
docker run -d -p 6379:6379 redis:7-alpine
```

### 4. Apply Database Migrations

```bash
cd Maliev.PerformanceService.Infrastructure
dotnet ef database update --project ../Maliev.PerformanceService.Api
```

### 5. Run the Service

```bash
cd Maliev.PerformanceService.Api
dotnet run
```

**Service Endpoints**:
- API: `http://localhost:8080/performance/v1`
- Health: `http://localhost:8080/performance/liveness`
- Metrics: `http://localhost:8080/performance/metrics`
- API Docs: `http://localhost:8080/performance/swagger`

---

## Common Operations

### Create a Performance Review

```bash
curl -X POST http://localhost:8080/performance/v1/employees/{employeeId}/reviews \
  -H "Authorization: Bearer {jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "reviewCycle": "Annual",
    "reviewPeriodStart": "2024-01-01T00:00:00Z",
    "reviewPeriodEnd": "2024-12-31T23:59:59Z",
    "selfAssessment": "I have successfully completed all assigned projects..."
  }'
```

### Get Employee's Reviews

```bash
curl -X GET http://localhost:8080/performance/v1/employees/{employeeId}/reviews \
  -H "Authorization: Bearer {jwt-token}"
```

### Update Goal Progress

```bash
curl -X PUT http://localhost:8080/performance/v1/goals/{goalId}/progress \
  -H "Authorization: Bearer {jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "progressUpdate": "Completed AWS certification exam preparation",
    "completionStatus": "InProgress"
  }'
```

### Submit 360-Degree Feedback

```bash
curl -X POST http://localhost:8080/performance/v1/reviews/{reviewId}/feedback \
  -H "Authorization: Bearer {jwt-token}" \
  -H "Content-Type: application/json" \
  -d '{
    "feedbackType": "Peer",
    "feedback": "Great collaboration on the Q4 project",
    "isAnonymous": true
  }'
```

---

## Testing

### Run Unit Tests

```bash
cd Maliev.PerformanceService.Tests
dotnet test --filter Category=Unit
```

### Run Integration Tests (Testcontainers)

```bash
# Ensure Docker is running
dotnet test --filter Category=Integration
```

**Testcontainers** automatically spin up real PostgreSQL, RabbitMQ, and Redis containers for integration tests.

### Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

Target: 80%+ coverage for business-critical logic

---

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "Microsoft.AspNetCore.Watch.BrowserRefresh": "None",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.AspNetCore.Watch": "Warning",
      "System": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ExternalServices": {
    "EmployeeService": {
      "BaseUrl": "https://api.maliev.com/employee/v1"
    },
    "NotificationService": {
      "BaseUrl": "https://api.maliev.com/notification/v1"
    }
  },
  "BackgroundServices": {
    "PerformanceReviewReminder": {
      "CronSchedule": "0 8 * * *"
    },
    "PIPCheckInReminder": {
      "CronSchedule": "0 9 * * 1"
    }
  }
}
```

---

## Background Services

### Performance Review Reminder (Daily at 8 AM)

Sends reminders to:
- Employees with pending self-assessments
- Managers with pending reviews
- Notifications about upcoming review cycles

### PIP Check-In Reminder (Weekly on Monday at 9 AM)

Sends reminders to:
- Managers about scheduled PIP check-ins
- HR about PIPs nearing end dates

---

## Observability

### Structured Logging

All logs include:
- `TraceId`: Distributed tracing correlation ID
- `EmployeeId`: Encrypted employee identifier
- `ReviewId`: Review identifier (when applicable)
- `UserId`: Requesting user identifier

**Example Log Entry**:
```json
{
  "timestamp": "2025-12-28T10:15:30Z",
  "level": "Information",
  "message": "Performance review created successfully",
  "traceId": "00-abc123-def456-00",
  "employeeId": "hashed-employee-id",
  "reviewId": "123e4567-e89b-12d3-a456-426614174000",
  "userId": "987fcdeb-89ab-12d3-a456-426614174999"
}
```

### Key Metrics

- `http_server_request_duration_seconds` (P50, P95, P99 latency)
- `performance_reviews_completed_total` (counter, by rating)
- `performance_goals_completed_total` (counter, by status)
- `performance_pips_outcomes_total` (counter, by outcome)
- `performance_notifications_sent_total` (counter, success/failure)

### Distributed Tracing

Automatic trace propagation to:
- Employee Service HTTP calls
- Notification Service HTTP calls
- RabbitMQ message publishing/consumption

**Trace Context**: W3C Trace Context standard (`traceparent` header)

---

## Security

### Authentication

- JWT tokens issued by authentication service
- Tokens validated on every request
- User claims extracted for permission checks

### Authorization

Permission-based access control:

| Permission | Granted To | Actions |
|------------|-----------|---------|
| `performance.read` | Employee, Manager, HR | View reviews, goals |
| `performance.create` | Manager, HR | Create reviews |
| `performance.update` | Employee, Manager | Update reviews, goals |
| `performance.admin` | HR, Admin | Manage PIPs |
| `performance.feedback` | All authenticated users | Submit feedback |

**Resource-Scoped Authorization**:
- Employees can only view/update their own data
- Managers can view/update direct reports only
- HR can view/update any employee's data

### Data Encryption

- **In Transit**: TLS 1.2+ for all HTTP requests
- **At Rest**: PostgreSQL encryption at rest
- **Field-Level**: PII (employee names, IDs) encrypted using .NET Data Protection API
- **Key Management**: Encryption keys stored in Google Secret Manager, rotated every 90 days

---

## Error Handling

### Standard Error Response

```json
{
  "error": "REVIEW_PERIOD_OVERLAP",
  "message": "A review for this cycle already exists in this period",
  "timestamp": "2025-12-28T10:00:00Z"
}
```

### Common Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `REVIEW_PERIOD_OVERLAP` | 409 | Review period overlaps existing |
| `SELF_ASSESSMENT_REQUIRED` | 400 | Self-assessment not completed |
| `GOAL_INVALID_DATE` | 400 | Goal target date invalid |
| `PIP_ALREADY_ACTIVE` | 409 | Employee has active PIP |
| `NOT_AUTHORIZED` | 403 | Insufficient permissions |
| `REVIEW_NOT_FOUND` | 404 | Review not found |
| `DATA_VOLUME_LIMIT_REACHED` | 400 | Employee exceeded data volume limit |

### Circuit Breaker Behavior

External service calls protected by circuit breaker:
- **Failure Threshold**: 5 consecutive failures
- **Open Duration**: 30 seconds
- **Retry Logic**: Maximum 3 retries with exponential backoff (2s, 4s, 8s)
- **Timeout**: 10 seconds per request

**Graceful Degradation**:
- Core review operations succeed even if notification service is down (notifications queued)
- Read operations use cached employee data if Employee Service is unavailable
- Authentication failures block all requests (fail-secure)

---

## Data Volume Limits

| Entity | Limit Per Employee | Soft Warning (80%) | Action at Limit |
|--------|-------------------|-------------------|-----------------|
| Performance Reviews | 50 | 40 | Block creation, error message to archive |
| Goals | 100 | 80 | Block creation, error message to archive |
| Feedback (aggregate) | 200 | 160 | Block creation, error message to archive |

**Handling Limit Warnings**:
- HR receives notification when employee approaches limit
- Archival job automatically moves 7+ year old reviews to cold storage
- Manual archival can be triggered via admin endpoint

---

## Deployment

### Docker Build

```bash
cd Maliev.PerformanceService.Api
docker build -t gcr.io/maliev/performance-service:latest \
  --secret id=nuget_username,env=NUGET_USERNAME \
  --secret id=nuget_password,env=NUGET_PASSWORD \
  .
```

### Kubernetes Deployment

```bash
kubectl apply -f k8s/deployment.yaml
```

**Resource Requests/Limits**:
- Memory: 256MB request, 512MB limit
- CPU: 100m request, 500m limit

### Health Checks

- **Liveness**: `/performance/liveness` (returns 200 OK if service is running)
- **Readiness**: `/performance/readiness` (returns 200 OK if dependencies are healthy)

---

## Next Steps

1. **Read the Specification**: [spec.md](./spec.md) for detailed requirements
2. **Review the Plan**: [plan.md](./plan.md) for architecture and technical decisions
3. **Explore the Data Model**: [data-model.md](./data-model.md) for entity relationships
4. **Check the API Contract**: [contracts/openapi.yaml](./contracts/openapi.yaml) for endpoint details
5. **Run Tests**: Ensure all tests pass before making changes
6. **Implement Tasks**: Use `/speckit.tasks` to generate implementation tasks

---

## Support & Resources

- **Documentation**: See `specs/001-performance-service/` directory
- **API Explorer**: `http://localhost:8080/performance/swagger`
- **Constitution**: `.specify/memory/constitution.md` for coding standards
- **Issue Tracker**: GitHub Issues for bug reports and feature requests

---

**Version**: 1.0.0
**Last Updated**: 2025-12-28
