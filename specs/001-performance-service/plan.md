# Implementation Plan: Performance Management Service

**Branch**: `001-performance-service` | **Date**: 2025-12-28 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-performance-service/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

The Performance Management Service is a microservice responsible for managing employee performance reviews, goals, performance improvement plans (PIPs), and 360-degree feedback. It provides comprehensive lifecycle management for performance evaluations, including self-assessments, manager reviews, acknowledgments, goal tracking with progress updates, and PIP workflows with check-ins and outcomes. The service integrates with the Employee Service for employee data, publishes integration events for key performance milestones, and implements background services for automated reminders and notifications.

**Technical Approach**: ASP.NET Core 10.0 microservice using PostgreSQL for persistence, RabbitMQ with MassTransit for event-driven integration, Redis for caching, and EF Core for ORM. The service follows CQRS patterns with explicit command/query handlers, implements circuit breakers and retry logic for external service resilience, and provides structured logging, metrics, and distributed tracing for observability.

## Technical Context

**Language/Version**: .NET 10.0 with C# 13
**Framework**: ASP.NET Core 10.0
**Primary Dependencies**:
- Entity Framework Core 10.x (ORM)
- MassTransit (messaging abstraction for RabbitMQ)
- Maliev.Aspire.ServiceDefaults (shared observability, health checks, resilience)
- Data Annotations (validation)
- Testcontainers (.NET) for integration testing

**Storage**: PostgreSQL 18 (primary database), Redis 7.x (distributed cache)
**Messaging**: RabbitMQ via MassTransit (event publishing and consumption)
**Testing**: xUnit with Testcontainers (real PostgreSQL, Redis, RabbitMQ containers)
**Target Platform**: Linux containers on Kubernetes (GKE)
**Project Type**: Microservice (4 projects: Api, Application, Domain, Infrastructure, Tests)
**Performance Goals**:
- Review operations complete in <2 seconds (SC-005)
- System supports 500 concurrent users without degradation (SC-004)
- Goal updates visible within 5 seconds (SC-006)
- Integration events published within 10 seconds (SC-010)
- 99.9% uptime (SC-009)

**Constraints**:
- Container memory: 256MB request, 512MB limit
- Encryption at rest and in transit for all performance data (FR-065, FR-066)
- Field-level encryption for PII (FR-067)
- Data volume limits: 50 reviews, 100 goals, 200 feedback entries per employee (FR-074)
- 7-year data retention with archival to cold storage (FR-059)
- Circuit breakers for all external service calls (FR-069)

**Scale/Scope**:
- 500 concurrent users during peak review periods
- Supports Annual, Semi-Annual, Quarterly, and Probation review cycles
- Multiple review cycles per employee over employment tenure
- 360-degree feedback from multiple providers per review
- Background services for daily reminders (reviews) and weekly reminders (PIPs)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Service Autonomy ✅ PASS
- **Own Database**: PostgreSQL database dedicated to Performance Service
- **Own Domain Logic**: All performance management business rules encapsulated
- **API/Event Integration**: Consumes EmployeeCreatedEvent, EmployeeTerminatedEvent; publishes PerformanceReviewCreatedEvent, PerformanceReviewAcknowledgedEvent, GoalCompletedEvent, PIPInitiatedEvent, PIPCompletedEvent
- **No Direct DB Access**: Uses HTTP clients with circuit breakers for Employee Service, Notification Service

### Explicit Contracts ✅ PASS
- OpenAPI/Scalar documentation for all REST endpoints
- Versioned event contracts for published/consumed events
- Backward-compatible schema migrations planned

### Test-First Development ✅ PASS
- Tests will be authored after spec approval, before implementation
- Testcontainers for real PostgreSQL, Redis, RabbitMQ
- Unit tests for business logic, integration tests for API + database + messaging
- Target 80%+ coverage for business-critical logic

### Real Infrastructure Testing ✅ PASS
- Testcontainers for PostgreSQL (no EF InMemory provider)
- Testcontainers for RabbitMQ (no in-memory message bus)
- Testcontainers for Redis (no in-memory cache)

### Auditability & Observability ✅ PASS
- Structured JSON logging with correlation IDs (FR-060)
- Key metrics tracked: request latency, throughput, error rates, review completion rates, notification delivery rates (FR-061)
- Distributed tracing for cross-service requests (FR-062)
- Health check endpoints for liveness/readiness (FR-064)
- Log level configuration in appsettings.json per constitution standards

### Security & Compliance ✅ PASS
- JWT authentication via `builder.AddJwtAuthentication()`
- Permission-based authorization (performance.create, performance.read, performance.update, performance.admin, performance.feedback)
- TLS 1.2+ for data in transit (FR-065)
- Encryption at rest for all performance data (FR-066)
- Field-level encryption for PII (FR-067)
- Encryption key management with rotation policies (FR-068)

### Secrets Management ✅ PASS
- Google Secret Manager integration via `builder.AddGoogleSecretManagerVolume()`
- No secrets in source code
- NuGet credentials via BuildKit secrets in Dockerfile

### Zero Warnings Policy ✅ PASS
- Build configured to treat warnings as errors

### Clean Project Artifacts ✅ PASS
- `.gitignore` excludes bin/, obj/, temporary files
- `.dockerignore` excludes build artifacts, specs, IDE files, Test projects
- No additional markdown files in repository root (only README.md)
- CODEOWNERS file at `.github/CODEOWNERS` with `* @MALIEV-Co-Ltd/core-developers`

### Docker Best Practices ✅ PASS
- Dockerfile in `Maliev.PerformanceService.Api/Dockerfile`
- Uses built-in `app` user from Microsoft ASP.NET runtime images
- Multi-stage build: SDK for build, ASP.NET runtime for final image
- .NET 10 base images: `mcr.microsoft.com/dotnet/sdk:10.0` and `mcr.microsoft.com/dotnet/aspnet:10.0`
- BuildKit secrets for NuGet credentials
- Health check validates `/performance/liveness` endpoint
- Port 8080 exposed

### .NET Aspire Integration ✅ PASS
- Consumes `Maliev.Aspire.ServiceDefaults` as NuGet package from GitHub Packages
- `nuget.config` with GitHub Packages source and credential placeholders
- `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()` in Program.cs
- Docker BuildKit secrets for NuGet authentication

### Code Quality & Library Standards ✅ PASS
- **NO AutoMapper**: Explicit mapping in DTOs
- **NO FluentValidation**: Standard .NET DataAnnotations
- **NO FluentAssertions**: Standard xUnit `Assert`

### Project Structure & Naming ✅ PASS
- Flat structure: Projects at repository root (no `/src` or `/tests` folders)
- Naming convention: `Maliev.PerformanceService.Api`, `Maliev.PerformanceService.Application`, `Maliev.PerformanceService.Domain`, `Maliev.PerformanceService.Infrastructure`, `Maliev.PerformanceService.Tests`
- Dockerfile in API project folder

### CI/CD Standards ✅ PASS
- Workflow filenames: `ci-develop.yml`, `ci-staging.yml`, `ci-main.yml`
- Testcontainers for all integration tests (no docker-compose.yml)

### Business Metrics & Analytics ✅ PASS
- Metrics for review completion rates, goal tracking rates, PIP outcomes
- Structured format compatible with Prometheus/OpenTelemetry
- Tags: service_name, version, region, environment
- No PII exposure in metrics

## Project Structure

### Documentation (this feature)

```text
specs/001-performance-service/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── openapi.yaml
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Maliev.PerformanceService/
├── .github/
│   ├── workflows/
│   │   ├── ci-develop.yml
│   │   ├── ci-staging.yml
│   │   └── ci-main.yml
│   └── CODEOWNERS
├── Maliev.PerformanceService.Api/
│   ├── Controllers/
│   │   ├── PerformanceReviewsController.cs
│   │   ├── GoalsController.cs
│   │   ├── PIPsController.cs
│   │   └── FeedbackController.cs
│   ├── DTOs/
│   │   ├── PerformanceReviewDto.cs
│   │   ├── CreatePerformanceReviewRequest.cs
│   │   ├── GoalDto.cs
│   │   ├── CreateGoalRequest.cs
│   │   ├── UpdateGoalProgressRequest.cs
│   │   ├── PIPDto.cs
│   │   ├── CreatePIPRequest.cs
│   │   ├── FeedbackDto.cs
│   │   └── SubmitFeedbackRequest.cs
│   ├── Dockerfile
│   ├── Program.cs
│   ├── appsettings.json
│   └── Maliev.PerformanceService.Api.csproj
├── Maliev.PerformanceService.Application/
│   ├── Commands/
│   │   ├── CreatePerformanceReviewCommand.cs
│   │   ├── UpdatePerformanceReviewCommand.cs
│   │   ├── SubmitPerformanceReviewCommand.cs
│   │   ├── AcknowledgePerformanceReviewCommand.cs
│   │   ├── CreateGoalCommand.cs
│   │   ├── UpdateGoalCommand.cs
│   │   ├── UpdateGoalProgressCommand.cs
│   │   ├── CreatePIPCommand.cs
│   │   ├── UpdatePIPCommand.cs
│   │   ├── RecordPIPOutcomeCommand.cs
│   │   └── SubmitFeedbackCommand.cs
│   ├── Handlers/
│   │   ├── CreatePerformanceReviewCommandHandler.cs
│   │   ├── UpdatePerformanceReviewCommandHandler.cs
│   │   ├── SubmitPerformanceReviewCommandHandler.cs
│   │   ├── AcknowledgePerformanceReviewCommandHandler.cs
│   │   ├── CreateGoalCommandHandler.cs
│   │   ├── UpdateGoalCommandHandler.cs
│   │   ├── UpdateGoalProgressCommandHandler.cs
│   │   ├── CreatePIPCommandHandler.cs
│   │   ├── UpdatePIPCommandHandler.cs
│   │   ├── RecordPIPOutcomeCommandHandler.cs
│   │   ├── SubmitFeedbackCommandHandler.cs
│   │   ├── GetPerformanceReviewsQueryHandler.cs
│   │   ├── GetPerformanceReviewByIdQueryHandler.cs
│   │   ├── GetGoalsQueryHandler.cs
│   │   ├── GetGoalByIdQueryHandler.cs
│   │   ├── GetPIPsQueryHandler.cs
│   │   └── GetFeedbackQueryHandler.cs
│   ├── Queries/
│   │   ├── GetPerformanceReviewsQuery.cs
│   │   ├── GetPerformanceReviewByIdQuery.cs
│   │   ├── GetGoalsQuery.cs
│   │   ├── GetGoalByIdQuery.cs
│   │   ├── GetPIPsQuery.cs
│   │   └── GetFeedbackQuery.cs
│   ├── Validators/
│   │   ├── CreatePerformanceReviewValidator.cs
│   │   ├── CreateGoalValidator.cs
│   │   ├── UpdateGoalProgressValidator.cs
│   │   ├── CreatePIPValidator.cs
│   │   └── SubmitFeedbackValidator.cs
│   ├── Interfaces/
│   │   ├── IPerformanceReviewRepository.cs
│   │   ├── IGoalRepository.cs
│   │   ├── IPIPRepository.cs
│   │   ├── IFeedbackRepository.cs
│   │   ├── IEmployeeServiceClient.cs
│   │   └── INotificationServiceClient.cs
│   └── Maliev.PerformanceService.Application.csproj
├── Maliev.PerformanceService.Domain/
│   ├── Entities/
│   │   ├── PerformanceReview.cs
│   │   ├── Goal.cs
│   │   ├── PerformanceImprovementPlan.cs
│   │   └── ReviewFeedback.cs
│   ├── Enums/
│   │   ├── ReviewCycle.cs
│   │   ├── PerformanceRating.cs
│   │   ├── ReviewStatus.cs
│   │   ├── GoalStatus.cs
│   │   ├── PIPStatus.cs
│   │   ├── PIPOutcome.cs
│   │   └── FeedbackType.cs
│   ├── Events/
│   │   ├── PerformanceReviewCreatedEvent.cs
│   │   ├── PerformanceReviewAcknowledgedEvent.cs
│   │   ├── GoalCompletedEvent.cs
│   │   ├── PIPInitiatedEvent.cs
│   │   ├── PIPCompletedEvent.cs
│   │   ├── EmployeeCreatedEvent.cs
│   │   └── EmployeeTerminatedEvent.cs
│   ├── Authorization/
│   │   └── PerformancePermissions.cs
│   └── Maliev.PerformanceService.Domain.csproj
├── Maliev.PerformanceService.Infrastructure/
│   ├── Data/
│   │   ├── PerformanceDbContext.cs
│   │   └── Configurations/
│   │       ├── PerformanceReviewConfiguration.cs
│   │       ├── GoalConfiguration.cs
│   │       ├── PerformanceImprovementPlanConfiguration.cs
│   │       └── ReviewFeedbackConfiguration.cs
│   ├── Repositories/
│   │   ├── PerformanceReviewRepository.cs
│   │   ├── GoalRepository.cs
│   │   ├── PIPRepository.cs
│   │   └── FeedbackRepository.cs
│   ├── BackgroundServices/
│   │   ├── PerformanceReviewReminderBackgroundService.cs
│   │   └── PIPCheckInReminderBackgroundService.cs
│   ├── Consumers/
│   │   ├── EmployeeCreatedEventConsumer.cs
│   │   └── EmployeeTerminatedEventConsumer.cs
│   ├── Clients/
│   │   ├── EmployeeServiceClient.cs
│   │   └── NotificationServiceClient.cs
│   ├── IAM/
│   │   └── PerformanceIAMRegistrationService.cs
│   ├── Migrations/
│   └── Maliev.PerformanceService.Infrastructure.csproj
├── Maliev.PerformanceService.Tests/
│   ├── Unit/
│   │   ├── Handlers/
│   │   ├── Validators/
│   │   └── Entities/
│   ├── Integration/
│   │   ├── Controllers/
│   │   ├── Repositories/
│   │   ├── BackgroundServices/
│   │   └── Consumers/
│   ├── TestFixtures/
│   │   ├── PostgreSqlFixture.cs
│   │   ├── RabbitMqFixture.cs
│   │   └── RedisFixture.cs
│   └── Maliev.PerformanceService.Tests.csproj
├── nuget.config
├── .gitignore
├── .dockerignore
├── README.md
└── Maliev.PerformanceService.sln
```

**Structure Decision**: Microservice with 4 projects following clean architecture principles. The API project contains controllers and DTOs for REST endpoints. The Application project contains CQRS command/query handlers, validators, and repository interfaces. The Domain project contains entities, enums, events, and authorization constants. The Infrastructure project contains EF Core DbContext, repository implementations, background services, event consumers, external service clients, and IAM registration. The Tests project uses Testcontainers for integration tests with real PostgreSQL, Redis, and RabbitMQ instances.

## Complexity Tracking

**No constitutional violations requiring justification.**

All design choices align with the MALIEV Microservices Constitution:
- 4 projects (Api, Application, Domain, Infrastructure) are standard for clean architecture microservices
- Repository pattern provides abstraction over EF Core for testing and future persistence changes
- CQRS pattern with explicit handlers improves testability and separation of concerns
- External service clients encapsulated behind interfaces for circuit breaker injection and testing
- Background services for scheduled reminders are standard hosted services in ASP.NET Core
