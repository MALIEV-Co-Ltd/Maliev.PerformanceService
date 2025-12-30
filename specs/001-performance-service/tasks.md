---
description: "Implementation task list for Performance Management Service"
---

# Tasks: Performance Management Service

**Input**: Design documents from `/specs/001-performance-service/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/openapi.yaml ✅

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Project structure follows flat layout at repository root:
- `Maliev.PerformanceService.Api/` - Controllers, DTOs, Program.cs, Dockerfile
- `Maliev.PerformanceService.Application/` - Commands, Queries, Handlers, Validators
- `Maliev.PerformanceService.Domain/` - Entities, Enums, Events, Authorization
- `Maliev.PerformanceService.Infrastructure/` - DbContext, Repositories, Clients, Background Services
- `Maliev.PerformanceService.Tests/` - Unit and Integration tests

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution file Maliev.PerformanceService.sln at repository root
- [X] T002 [P] Create Maliev.PerformanceService.Api project with ASP.NET Core 10.0 web API template
- [X] T003 [P] Create Maliev.PerformanceService.Application class library project targeting .NET 10
- [X] T004 [P] Create Maliev.PerformanceService.Domain class library project targeting .NET 10
- [X] T005 [P] Create Maliev.PerformanceService.Infrastructure class library project targeting .NET 10
- [X] T006 [P] Create Maliev.PerformanceService.Tests xUnit test project targeting .NET 10
- [X] T007 Add project references: Api -> Application, Infrastructure; Application -> Domain; Infrastructure -> Application, Domain; Tests -> Api, Application, Domain, Infrastructure
- [X] T008 Create nuget.config with GitHub Packages source and credential placeholders for Maliev.Aspire.ServiceDefaults
- [X] T009 [P] Create .gitignore excluding bin/, obj/, .vs/, .idea/, *.user, *.suo, appsettings.Development.json
- [X] T010 [P] Create .dockerignore excluding **/.git, **/bin, **/obj, specs/, .specify/, .claude/, .gemini/, *.Tests/
- [X] T011 [P] Create .github/CODEOWNERS with "* @MALIEV-Co-Ltd/core-developers"
- [X] T012 [P] Create .github/workflows/ci-develop.yml for develop branch CI pipeline with build, test, Docker build
- [X] T013 [P] Create .github/workflows/ci-staging.yml for staging branch CI pipeline with deployment to staging environment
- [X] T014 [P] Create .github/workflows/ci-main.yml for main branch CI pipeline with deployment to production environment
- [X] T015 Configure build to treat warnings as errors in all .csproj files (<TreatWarningsAsErrors>true</TreatWarningsAsErrors>)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Domain Layer (Entities, Enums, Events, Authorization)

- [X] T016 [P] Create ReviewCycle enum in Maliev.PerformanceService.Domain/Enums/ReviewCycle.cs (Annual, SemiAnnual, Quarterly, Probation)
- [X] T017 [P] Create PerformanceRating enum in Maliev.PerformanceService.Domain/Enums/PerformanceRating.cs (ExceedsExpectations, MeetsExpectations, NeedsImprovement, BelowExpectations, Unsatisfactory)
- [X] T018 [P] Create ReviewStatus enum in Maliev.PerformanceService.Domain/Enums/ReviewStatus.cs (Draft, SelfAssessmentPending, ManagerReviewPending, Submitted, Acknowledged, Completed)
- [X] T019 [P] Create GoalStatus enum in Maliev.PerformanceService.Domain/Enums/GoalStatus.cs (NotStarted, InProgress, AtRisk, Completed, Deferred, Cancelled)
- [X] T020 [P] Create PIPStatus enum in Maliev.PerformanceService.Domain/Enums/PIPStatus.cs (Active, Extended, Completed, Terminated)
- [X] T021 [P] Create PIPOutcome enum in Maliev.PerformanceService.Domain/Enums/PIPOutcome.cs (Successful, Unsuccessful, ExtendedAgain)
- [X] T022 [P] Create FeedbackType enum in Maliev.PerformanceService.Domain/Enums/FeedbackType.cs (Manager, Peer, DirectReport, Self)
- [X] T023 [P] Create PerformanceReview entity in Maliev.PerformanceService.Domain/Entities/PerformanceReview.cs with properties: Id (Guid), EmployeeId (Guid), ReviewerId (Guid), ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd, SelfAssessment, ManagerAssessment, OverallRating, Status, SubmittedDate, AcknowledgedDate, CreatedDate, ModifiedDate
- [X] T024 [P] Create Goal entity in Maliev.PerformanceService.Domain/Entities/Goal.cs with properties: Id (Guid), EmployeeId (Guid), PerformanceReviewId (Guid?), Description, SuccessCriteria, TargetCompletionDate, CurrentStatus, ProgressUpdates (text append-only), CompletionDate, CreatedDate, ModifiedDate
- [X] T025 [P] Create ReviewFeedback entity in Maliev.PerformanceService.Domain/Entities/ReviewFeedback.cs with properties: Id (Guid), PerformanceReviewId (Guid), ProviderId (Guid), FeedbackType, Feedback (text), IsAnonymous (bool), SubmittedDate, CreatedDate
- [X] T026 [P] Create PerformanceImprovementPlan entity in Maliev.PerformanceService.Domain/Entities/PerformanceImprovementPlan.cs with properties: Id (Guid), EmployeeId (Guid), InitiatorId (Guid), StartDate, EndDate, Reason, ImprovementAreas, SuccessCriteria, CheckInNotes (text append-only), Status, Outcome, ExtensionCount (int), CreatedDate, ModifiedDate
- [X] T027 [P] Create PerformanceReviewCreatedEvent in Maliev.PerformanceService.Domain/Events/PerformanceReviewCreatedEvent.cs with properties: ReviewId, EmployeeId, ReviewerId, ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd, CreatedDate
- [X] T028 [P] Create PerformanceReviewAcknowledgedEvent in Maliev.PerformanceService.Domain/Events/PerformanceReviewAcknowledgedEvent.cs with properties: ReviewId, EmployeeId, OverallRating, AcknowledgedDate
- [X] T029 [P] Create GoalCompletedEvent in Maliev.PerformanceService.Domain/Events/GoalCompletedEvent.cs with properties: GoalId, EmployeeId, Description, CompletionDate
- [X] T030 [P] Create PIPInitiatedEvent in Maliev.PerformanceService.Domain/Events/PIPInitiatedEvent.cs with properties: PIPId, EmployeeId, InitiatorId, StartDate, EndDate, Reason
- [X] T031 [P] Create PIPCompletedEvent in Maliev.PerformanceService.Domain/Events/PIPCompletedEvent.cs with properties: PIPId, EmployeeId, Outcome, CompletedDate
- [X] T032 [P] Create EmployeeCreatedEvent in Maliev.PerformanceService.Domain/Events/EmployeeCreatedEvent.cs (consumed event per MessagingContracts) with properties: EmployeeId, EmployeeNumber, StartDate, DepartmentId, PositionId?, ManagerId?
- [X] T033 [P] Create EmployeeTerminatedEvent in Maliev.PerformanceService.Domain/Events/EmployeeTerminatedEvent.cs (consumed event per MessagingContracts) with properties: EmployeeId, TerminationDate, TerminationReason?, EligibleForRehire
- [X] T034 Create PerformancePermissions static class in Maliev.PerformanceService.Domain/Authorization/PerformancePermissions.cs with constants: Create, Read, Update, Admin, Feedback

### Application Layer (Interfaces)

- [X] T035 [P] Create IPerformanceReviewRepository interface in Maliev.PerformanceService.Application/Interfaces/IPerformanceReviewRepository.cs with methods: GetByIdAsync, GetByEmployeeIdAsync, CreateAsync, UpdateAsync, DeleteAsync, ExistsOverlappingReviewAsync
- [X] T036 [P] Create IGoalRepository interface in Maliev.PerformanceService.Application/Interfaces/IGoalRepository.cs with methods: GetByIdAsync, GetByEmployeeIdAsync, CreateAsync, UpdateAsync, DeleteAsync
- [X] T037 [P] Create IPIPRepository interface in Maliev.PerformanceService.Application/Interfaces/IPIPRepository.cs with methods: GetByIdAsync, GetByEmployeeIdAsync, CreateAsync, UpdateAsync, GetActivePIPByEmployeeIdAsync
- [X] T038 [P] Create IFeedbackRepository interface in Maliev.PerformanceService.Application/Interfaces/IFeedbackRepository.cs with methods: GetByReviewIdAsync, CreateAsync, GetFeedbackCountByTypeAsync
- [X] T039 [P] Create IEmployeeServiceClient interface in Maliev.PerformanceService.Application/Interfaces/IEmployeeServiceClient.cs with methods: GetEmployeeByIdAsync, ValidateEmployeeExistsAsync, ValidateManagerEmployeeRelationshipAsync
- [X] T040 [P] Create INotificationServiceClient interface in Maliev.PerformanceService.Application/Interfaces/INotificationServiceClient.cs with methods: SendReviewReminderAsync, SendPIPCheckInReminderAsync, SendAcknowledgmentReminderAsync

### Infrastructure Layer (Database, Repositories, Clients)

- [X] T041 Add NuGet package references to Infrastructure project: EntityFrameworkCore 10.x, EntityFrameworkCore.Design, Npgsql.EntityFrameworkCore.PostgreSQL, MassTransit.RabbitMQ, Maliev.Aspire.ServiceDefaults, Microsoft.AspNetCore.DataProtection, Polly
- [X] T042 Create PerformanceDbContext in Maliev.PerformanceService.Infrastructure/Data/PerformanceDbContext.cs with DbSet properties for PerformanceReview, Goal, ReviewFeedback, PerformanceImprovementPlan
- [X] T043 [P] Create PerformanceReviewConfiguration in Maliev.PerformanceService.Infrastructure/Data/Configurations/PerformanceReviewConfiguration.cs implementing IEntityTypeConfiguration<PerformanceReview> with table name, primary key, indexes (employee_id, unique index for overlap prevention), constraints (review period check)
- [X] T044 [P] Create GoalConfiguration in Maliev.PerformanceService.Infrastructure/Data/Configurations/GoalConfiguration.cs implementing IEntityTypeConfiguration<Goal> with table name, primary key, indexes (employee_id, performance_review_id), constraints (target date check)
- [X] T045 [P] Create PerformanceImprovementPlanConfiguration in Maliev.PerformanceService.Infrastructure/Data/Configurations/PerformanceImprovementPlanConfiguration.cs implementing IEntityTypeConfiguration<PerformanceImprovementPlan> with table name, primary key, indexes (employee_id), constraints (date range check)
- [X] T046 [P] Create ReviewFeedbackConfiguration in Maliev.PerformanceService.Infrastructure/Data/Configurations/ReviewFeedbackConfiguration.cs implementing IEntityTypeConfiguration<ReviewFeedback> with table name, primary key, indexes (performance_review_id, feedback_type), field-level encryption for ProviderId when IsAnonymous=true
- [X] T047 Apply entity configurations in PerformanceDbContext.OnModelCreating method
- [X] T048 Create initial database migration using dotnet ef migrations add InitialCreate --project Maliev.PerformanceService.Infrastructure --startup-project Maliev.PerformanceService.Api
- [X] T049 [P] Create PerformanceReviewRepository in Maliev.PerformanceService.Infrastructure/Repositories/PerformanceReviewRepository.cs implementing IPerformanceReviewRepository with EF Core queries including ExistsOverlappingReviewAsync with WHERE status != Completed filter
- [X] T050 [P] Create GoalRepository in Maliev.PerformanceService.Infrastructure/Repositories/GoalRepository.cs implementing IGoalRepository with cursor-based pagination support
- [X] T051 [P] Create PIPRepository in Maliev.PerformanceService.Infrastructure/Repositories/PIPRepository.cs implementing IPIPRepository with GetActivePIPByEmployeeIdAsync filtering by status=Active or Extended
- [X] T052 [P] Create FeedbackRepository in Maliev.PerformanceService.Infrastructure/Repositories/FeedbackRepository.cs implementing IFeedbackRepository with anonymity enforcement (hash ProviderId when IsAnonymous=true)
- [X] T053 [P] Create EmployeeServiceClient in Maliev.PerformanceService.Infrastructure/Clients/EmployeeServiceClient.cs implementing IEmployeeServiceClient with Polly circuit breaker, retry policies, and Redis caching for GetEmployeeByIdAsync
- [X] T054 [P] Create NotificationServiceClient in Maliev.PerformanceService.Infrastructure/Clients/NotificationServiceClient.cs implementing INotificationServiceClient with Polly circuit breaker and retry policies
- [X] T055 Create IAM registration service in Maliev.PerformanceService.Infrastructure/IAM/PerformanceIAMRegistrationService.cs as IHostedService to register performance.* permissions with IAM service on startup

### API Layer (Authentication, Configuration, Middleware)

- [X] T056 Add NuGet package references to Api project: Maliev.Aspire.ServiceDefaults, MassTransit.RabbitMQ, Scalar.AspNetCore (OpenAPI docs)
- [X] T057 Configure Program.cs in Maliev.PerformanceService.Api/Program.cs with builder.AddServiceDefaults(), builder.AddJwtAuthentication(), builder.AddGoogleSecretManagerVolume(), DbContext registration, MassTransit with RabbitMQ, repository registrations, HTTP client registrations with circuit breakers
- [X] T058 Configure Program.cs middleware pipeline with app.UseAuthentication(), app.UseAuthorization(), app.MapDefaultEndpoints(), app.MapControllers()
- [X] T059 Create appsettings.json in Maliev.PerformanceService.Api/ with logging configuration (per constitution standards), ExternalServices (EmployeeService, NotificationService), BackgroundServices cron schedules, AllowedHosts
- [X] T060 Configure structured logging with correlation ID, employee ID, review ID, user ID in all log entries

### Deployment Configuration

- [X] T061 Create Dockerfile in Maliev.PerformanceService.Api/Dockerfile with multi-stage build (mcr.microsoft.com/dotnet/sdk:10.0 for build, mcr.microsoft.com/dotnet/aspnet:10.0 for runtime), BuildKit secrets for NuGet credentials, HEALTHCHECK /performance/liveness, port 8080 exposed, app user
- [X] T062 Create k8s/deployment.yaml with resource requests (256MB memory, 100m CPU) and limits (512MB memory, 500m CPU), liveness probe /performance/liveness, readiness probe /performance/readiness, environment variables from ConfigMap and Secrets
- [X] T063 Create k8s/service.yaml exposing port 8080 with ClusterIP service type
- [X] T064 Create k8s/configmap.yaml with non-sensitive configuration values

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Employee Self-Review Submission (Priority: P1) 🎯 MVP

**Goal**: Enable employees to complete and submit their performance self-assessments for review cycles

**Independent Test**: Create a review period for an employee, submit self-assessment, verify submission is saved and visible to manager

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [X] T065 [P] [US1] Create unit test for CreatePerformanceReviewValidator in Maliev.PerformanceService.Tests/Unit/Validators/CreatePerformanceReviewValidatorTests.cs testing review period validation, cycle validation, required fields
- [X] T066 [P] [US1] Create unit test for CreatePerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/CreatePerformanceReviewCommandHandlerTests.cs testing overlap detection, employee validation, event publishing
- [X] T067 [P] [US1] Create unit test for UpdatePerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/UpdatePerformanceReviewCommandHandlerTests.cs testing self-assessment updates, draft saves, status transitions
- [X] T068 [US1] Create integration test for PerformanceReviewsController.CreateReview endpoint in Maliev.PerformanceService.Tests/Integration/Controllers/PerformanceReviewsControllerTests.cs using Testcontainers for PostgreSQL, RabbitMQ - verify review creation, overlap prevention, event publishing
- [X] T069 [US1] Create integration test for PerformanceReviewsController.UpdateReview endpoint in Maliev.PerformanceService.Tests/Integration/Controllers/PerformanceReviewsControllerTests.cs - verify self-assessment submission, status change to SelfAssessmentPending

### Implementation for User Story 1

- [X] T070 [P] [US1] Create PerformanceReviewDto in Maliev.PerformanceService.Api/DTOs/PerformanceReviewDto.cs with properties for review data (Id, EmployeeId, ReviewerId, ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd, SelfAssessment, ManagerAssessment, OverallRating, Status, SubmittedDate, AcknowledgedDate)
- [X] T071 [P] [US1] Create CreatePerformanceReviewRequest DTO in Maliev.PerformanceService.Api/DTOs/CreatePerformanceReviewRequest.cs with DataAnnotations validation ([Required] ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd; [MaxLength] SelfAssessment)
- [X] T072 [P] [US1] Create UpdatePerformanceReviewRequest DTO in Maliev.PerformanceService.Api/DTOs/UpdatePerformanceReviewRequest.cs with optional SelfAssessment, ManagerAssessment fields
- [X] T073 [P] [US1] Create CreatePerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/CreatePerformanceReviewCommand.cs with record struct containing EmployeeId, ReviewCycle, ReviewPeriodStart, ReviewPeriodEnd, SelfAssessment, RequestingUserId
- [X] T074 [P] [US1] Create UpdatePerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/UpdatePerformanceReviewCommand.cs with record struct containing ReviewId, SelfAssessment, ManagerAssessment, RequestingUserId
- [X] T075 [P] [US1] Create GetPerformanceReviewsQuery in Maliev.PerformanceService.Application/Queries/GetPerformanceReviewsQuery.cs with record struct containing EmployeeId, RequestingUserId
- [X] T076 [P] [US1] Create GetPerformanceReviewByIdQuery in Maliev.PerformanceService.Application/Queries/GetPerformanceReviewByIdQuery.cs with record struct containing ReviewId, RequestingUserId
- [X] T077 [US1] Create CreatePerformanceReviewValidator in Maliev.PerformanceService.Application/Validators/CreatePerformanceReviewValidator.cs validating review period dates (start < end), cycle value, no overlap with existing reviews
- [X] T078 [US1] Create CreatePerformanceReviewCommandHandler in Maliev.PerformanceService.Application/Handlers/CreatePerformanceReviewCommandHandler.cs implementing command handling with: employee existence validation via IEmployeeServiceClient, overlap check via IPerformanceReviewRepository.ExistsOverlappingReviewAsync, review creation with status=Draft, publish PerformanceReviewCreatedEvent via MassTransit
- [X] T079 [US1] Create UpdatePerformanceReviewCommandHandler in Maliev.PerformanceService.Application/Handlers/UpdatePerformanceReviewCommandHandler.cs implementing self-assessment updates, draft saves, status transition to SelfAssessmentPending on submission, manager notification via INotificationServiceClient
- [X] T080 [US1] Create GetPerformanceReviewsQueryHandler in Maliev.PerformanceService.Application/Handlers/GetPerformanceReviewsQueryHandler.cs implementing authorization (employees see own reviews, managers see direct reports, HR sees all), retrieval via IPerformanceReviewRepository.GetByEmployeeIdAsync
- [X] T081 [US1] Create GetPerformanceReviewByIdQueryHandler in Maliev.PerformanceService.Application/Handlers/GetPerformanceReviewByIdQueryHandler.cs implementing resource-scoped authorization and single review retrieval
- [X] T082 [US1] Create PerformanceReviewsController in Maliev.PerformanceService.Api/Controllers/PerformanceReviewsController.cs with endpoints: POST /performance/v1/employees/{employeeId}/reviews (create review), GET /performance/v1/employees/{employeeId}/reviews (list reviews), GET /performance/v1/reviews/{reviewId} (get single review), PUT /performance/v1/reviews/{reviewId} (update review for self-assessment), all with [Authorize] attribute
- [X] T083 [US1] Implement mapping between DTOs and commands/queries in PerformanceReviewsController endpoints (explicit mapping, no AutoMapper)
- [X] T084 [US1] Add structured logging to CreatePerformanceReviewCommandHandler with correlation ID, employee ID, review ID, user ID
- [X] T085 [US1] Add error handling for REVIEW_PERIOD_OVERLAP, EMPLOYEE_NOT_FOUND, UNAUTHORIZED errors in CreatePerformanceReviewCommandHandler

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently - employees can create reviews and submit self-assessments

---

## Phase 4: User Story 2 - Manager Performance Review and Rating (Priority: P1)

**Goal**: Enable managers to review direct report self-assessments and provide their own assessment with performance rating

**Independent Test**: Manager reviews employee's submitted self-assessment, adds their assessment, assigns rating, submits for acknowledgment

### Tests for User Story 2

- [X] T086 [P] [US2] Create unit test for SubmitPerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/SubmitPerformanceReviewCommandHandlerTests.cs testing self-assessment requirement enforcement, rating validation, status transition to Submitted
- [X] T087 [US2] Create integration test for PerformanceReviewsController.SubmitReview endpoint in Maliev.PerformanceService.Tests/Integration/Controllers/PerformanceReviewsControllerTests.cs - verify manager can submit review only after employee self-assessment, employee notification sent, status changes to Submitted

### Implementation for User Story 2

- [X] T088 [P] [US2] Create SubmitPerformanceReviewRequest DTO in Maliev.PerformanceService.Api/DTOs/SubmitPerformanceReviewRequest.cs with [Required] ManagerAssessment and OverallRating (enum validation)
- [X] T089 [US2] Create SubmitPerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/SubmitPerformanceReviewCommand.cs with record struct containing ReviewId, ManagerAssessment, OverallRating, RequestingUserId
- [X] T090 [US2] Create SubmitPerformanceReviewCommandHandler in Maliev.PerformanceService.Application/Handlers/SubmitPerformanceReviewCommandHandler.cs implementing: validation that self-assessment is completed (block if status != SelfAssessmentPending), manager authorization check (via manager-employee relationship validation), update manager assessment and rating, status transition to Submitted, send acknowledgment notification via INotificationServiceClient
- [X] T091 [US2] Add POST /performance/v1/reviews/{reviewId}/submit endpoint to PerformanceReviewsController in Maliev.PerformanceService.Api/Controllers/PerformanceReviewsController.cs with manager authorization check
- [X] T092 [US2] Add error handling for SELF_ASSESSMENT_REQUIRED error in SubmitPerformanceReviewCommandHandler with clear message
- [X] T093 [US2] Add structured logging to SubmitPerformanceReviewCommandHandler with rating, manager ID, submission timestamp

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - complete review lifecycle from employee self-assessment to manager submission

---

## Phase 5: User Story 3 - Goal Setting and Tracking (Priority: P2)

**Goal**: Enable employees and managers to set goals, track progress with updates, and mark goals as completed

**Independent Test**: Create goal for employee, add progress updates, mark as completed, verify completion date recorded

### Tests for User Story 3

- [X] T094 [P] [US3] Create unit test for CreateGoalValidator in Maliev.PerformanceService.Tests/Unit/Validators/CreateGoalValidatorTests.cs testing target date validation (future date), required fields
- [X] T095 [P] [US3] Create unit test for CreateGoalCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/CreateGoalCommandHandlerTests.cs testing goal creation, review period linkage, employee validation
- [X] T096 [P] [US3] Create unit test for UpdateGoalProgressCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/UpdateGoalProgressCommandHandlerTests.cs testing append-only progress updates, status transitions, completion event publishing
- [X] T097 [US3] Create integration test for GoalsController endpoints in Maliev.PerformanceService.Tests/Integration/Controllers/GoalsControllerTests.cs using Testcontainers - verify goal creation, progress tracking, completion with event publishing, status=AtRisk alert sending

### Implementation for User Story 3

- [X] T098 [P] [US3] Create GoalDto in Maliev.PerformanceService.Api/DTOs/GoalDto.cs with properties: Id, EmployeeId, PerformanceReviewId, Description, SuccessCriteria, TargetCompletionDate, CurrentStatus, ProgressUpdates, CompletionDate
- [X] T099 [P] [US3] Create CreateGoalRequest DTO in Maliev.PerformanceService.Api/DTOs/CreateGoalRequest.cs with [Required] Description, SuccessCriteria, TargetCompletionDate; optional PerformanceReviewId
- [X] T100 [P] [US3] Create UpdateGoalProgressRequest DTO in Maliev.PerformanceService.Api/DTOs/UpdateGoalProgressRequest.cs with [Required] ProgressUpdate (string), [Required] CompletionStatus (GoalStatus enum)
- [X] T101 [P] [US3] Create CreateGoalCommand in Maliev.PerformanceService.Application/Commands/CreateGoalCommand.cs with record struct
- [X] T102 [P] [US3] Create UpdateGoalCommand in Maliev.PerformanceService.Application/Commands/UpdateGoalCommand.cs for general updates
- [X] T103 [P] [US3] Create UpdateGoalProgressCommand in Maliev.PerformanceService.Application/Commands/UpdateGoalProgressCommand.cs with record struct containing GoalId, ProgressUpdate, CompletionStatus, RequestingUserId
- [X] T104 [P] [US3] Create GetGoalsQuery in Maliev.PerformanceService.Application/Queries/GetGoalsQuery.cs with record struct containing EmployeeId, RequestingUserId
- [X] T105 [P] [US3] Create GetGoalByIdQuery in Maliev.PerformanceService.Application/Queries/GetGoalByIdQuery.cs with record struct
- [X] T106 [US3] Create CreateGoalValidator in Maliev.PerformanceService.Application/Validators/CreateGoalValidator.cs validating target date is in future, description length, success criteria presence
- [X] T107 [US3] Create UpdateGoalProgressValidator in Maliev.PerformanceService.Application/Validators/UpdateGoalProgressValidator.cs validating progress update is not empty, valid status transitions
- [X] T108 [US3] Create CreateGoalCommandHandler in Maliev.PerformanceService.Application/Handlers/CreateGoalCommandHandler.cs implementing employee validation, optional review period linkage validation, goal creation with status=NotStarted
- [X] T109 [X] [US3] Create UpdateGoalCommandHandler in Maliev.PerformanceService.Application/Handlers/UpdateGoalCommandHandler.cs for general goal updates
- [X] T110 [US3] Create UpdateGoalProgressCommandHandler in Maliev.PerformanceService.Application/Handlers/UpdateGoalProgressCommandHandler.cs implementing: append-only progress update (timestamped), status update, completion date recording when status=Completed, publish GoalCompletedEvent via MassTransit when status changes to Completed, send AtRisk alert to manager via INotificationServiceClient when status=AtRisk
- [X] T111 [US3] Create GetGoalsQueryHandler in Maliev.PerformanceService.Application/Handlers/GetGoalsQueryHandler.cs implementing authorization and retrieval via IGoalRepository.GetByEmployeeIdAsync with cursor-based pagination
- [X] T112 [US3] Create GetGoalByIdQueryHandler in Maliev.PerformanceService.Application/Handlers/GetGoalByIdQueryHandler.cs with resource-scoped authorization
- [X] T113 [US3] Create GoalsController in Maliev.PerformanceService.Api/Controllers/GoalsController.cs with endpoints: POST /performance/v1/employees/{employeeId}/goals (create goal), GET /performance/v1/employees/{employeeId}/goals (list goals with pagination), GET /performance/v1/goals/{goalId} (get single goal), PUT /performance/v1/goals/{goalId}/progress (update progress), all with [Authorize] attribute
- [X] T114 [US3] Add error handling for GOAL_INVALID_DATE, GOAL_NOT_FOUND in goal handlers
- [X] T115 [US3] Add structured logging to goal handlers with goal ID, employee ID, status changes

**Checkpoint**: At this point, User Stories 1, 2, AND 3 should all work independently - employees can manage goals alongside their reviews

---

## Phase 6: User Story 4 - 360-Degree Feedback Collection (Priority: P3)

**Goal**: Enable peers, managers, and direct reports to submit feedback for employee reviews, with anonymity support

**Independent Test**: Submit feedback from multiple providers (some anonymous), verify feedback aggregated by type, anonymity preserved

### Tests for User Story 4

- [X] T116 [P] [US4] Create unit test for SubmitFeedbackCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/SubmitFeedbackCommandHandlerTests.cs testing anonymity enforcement (ProviderId hashing), feedback type validation
- [X] T117 [P] [US4] Create unit test for GetFeedbackQueryHandler in Maliev.PerformanceService.Tests/Unit/Handlers/GetFeedbackQueryHandlerTests.cs testing aggregation by feedback type, anonymity protection (suppression when only 1 provider of type)
- [X] T118 [US4] Create integration test for FeedbackController endpoints in Maliev.PerformanceService.Tests/Integration/Controllers/FeedbackControllerTests.cs - verify feedback submission, anonymity preservation (no ProviderId in response when IsAnonymous=true), aggregation by type, suppression when single provider

### Implementation for User Story 4

- [X] T119 [P] [US4] Create FeedbackDto in Maliev.PerformanceService.Api/DTOs/FeedbackDto.cs with properties: Id, PerformanceReviewId, ProviderId (null if anonymous), FeedbackType, Feedback, IsAnonymous, SubmittedDate
- [X] T120 [P] [US4] Create SubmitFeedbackRequest DTO in Maliev.PerformanceService.Api/DTOs/SubmitFeedbackRequest.cs with [Required] FeedbackType, Feedback, IsAnonymous (bool)
- [X] T121 [US4] Create SubmitFeedbackCommand in Maliev.PerformanceService.Application/Commands/SubmitFeedbackCommand.cs with record struct containing PerformanceReviewId, ProviderId, FeedbackType, Feedback, IsAnonymous, RequestingUserId
- [X] T122 [US4] Create GetFeedbackQuery in Maliev.PerformanceService.Application/Queries/GetFeedbackQuery.cs with record struct containing PerformanceReviewId, RequestingUserId
- [X] T123 [US4] Create SubmitFeedbackValidator in Maliev.PerformanceService.Application/Validators/SubmitFeedbackValidator.cs validating feedback text is not empty, valid feedback type
- [X] T124 [US4] Create SubmitFeedbackCommandHandler in Maliev.PerformanceService.Application/Handlers/SubmitFeedbackCommandHandler.cs implementing: review existence validation, feedback creation with ProviderId hashing if IsAnonymous=true (using .NET Data Protection API), warning if only 1 provider of this feedback type for review (cannot guarantee anonymity)
- [X] T125 [US4] Create GetFeedbackQueryHandler in Maliev.PerformanceService.Application/Handlers/GetFeedbackQueryHandler.cs implementing: authorization check (manager or employee being reviewed can view), retrieval via IFeedbackRepository.GetByReviewIdAsync, aggregation by FeedbackType, suppression of feedback when only 1 provider of that type and IsAnonymous=true
- [X] T126 [US4] Create FeedbackController in Maliev.PerformanceService.Api/Controllers/FeedbackController.cs with endpoints: POST /performance/v1/reviews/{reviewId}/feedback (submit feedback), GET /performance/v1/reviews/{reviewId}/feedback (get aggregated feedback), all with [Authorize] attribute
- [X] T127 [US4] Add structured logging to feedback handlers with review ID, feedback type, anonymity flag (but NOT provider ID if anonymous)

**Checkpoint**: At this point, User Stories 1-4 should all work independently - 360-degree feedback enriches performance reviews

---

## Phase 7: User Story 6 - Review Acknowledgment and Completion (Priority: P2)

**Goal**: Enable employees to acknowledge completed reviews, creating formal record of review communication

**Independent Test**: Employee views submitted review, acknowledges it, verify acknowledgment date recorded and status changes to Acknowledged

### Tests for User Story 6

- [X] T128 [P] [US6] Create unit test for AcknowledgePerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/AcknowledgePerformanceReviewCommandHandlerTests.cs testing authorization (only employee being reviewed can acknowledge), status transition to Acknowledged, event publishing
- [X] T129 [US6] Create integration test for PerformanceReviewsController.AcknowledgeReview endpoint in Maliev.PerformanceService.Tests/Integration/Controllers/PerformanceReviewsControllerTests.cs - verify employee can acknowledge review, acknowledgment date recorded, PerformanceReviewAcknowledgedEvent published

### Implementation for User Story 6

- [X] T130 [US6] Create AcknowledgePerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/AcknowledgePerformanceReviewCommand.cs with record struct containing ReviewId, RequestingUserId
- [X] T131 [US6] Create AcknowledgePerformanceReviewCommandHandler in Maliev.PerformanceService.Application/Handlers/AcknowledgePerformanceReviewCommandHandler.cs implementing: authorization check (requesting user must be the employee being reviewed, not manager or other employee), validation that review status is Submitted, record AcknowledgedDate, status transition to Acknowledged, publish PerformanceReviewAcknowledgedEvent via MassTransit
- [X] T132 [US6] Add POST /performance/v1/reviews/{reviewId}/acknowledge endpoint to PerformanceReviewsController in Maliev.PerformanceService.Api/Controllers/PerformanceReviewsController.cs with employee authorization
- [X] T133 [US6] Add structured logging to AcknowledgePerformanceReviewCommandHandler with review ID, employee ID, acknowledgment timestamp

**Checkpoint**: At this point, User Stories 1, 2, 3, 4, and 6 should all work independently - complete review lifecycle with acknowledgment

---

## Phase 8: User Story 5 - PIP Management (Priority: P4)

**Goal**: Enable HR and managers to initiate, track, and complete Performance Improvement Plans with check-ins and outcomes

**Independent Test**: Initiate PIP for employee, document check-ins, record outcome (successful/unsuccessful/extended), verify only one active PIP allowed per employee

### Tests for User Story 5

- [X] T134 [P] [US5] Create unit test for CreatePIPCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/CreatePIPCommandHandlerTests.cs testing duplicate active PIP prevention, date range validation, event publishing
- [X] T135 [P] [US5] Create unit test for UpdatePIPCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/UpdatePIPCommandHandlerTests.cs testing check-in note appending, status updates
- [X] T136 [P] [US5] Create unit test for RecordPIPOutcomeCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/RecordPIPOutcomeCommandHandlerTests.cs testing extension logic (maximum 1 extension), outcome recording, event publishing
- [X] T137 [US5] Create integration test for PIPsController endpoints in Maliev.PerformanceService.Tests/Integration/Controllers/PIPsControllerTests.cs using Testcontainers - verify PIP creation, duplicate prevention, check-in notes, extension logic (block after 1 extension), outcome recording with PIPCompletedEvent publishing

### Implementation for User Story 5

- [X] T138 [P] [US5] Create PIPDto in Maliev.PerformanceService.Api/DTOs/PIPDto.cs with properties: Id, EmployeeId, InitiatorId, StartDate, EndDate, Reason, ImprovementAreas, SuccessCriteria, CheckInNotes, Status, Outcome, ExtensionCount
- [X] T139 [P] [US5] Create CreatePIPRequest DTO in Maliev.PerformanceService.Api/DTOs/CreatePIPRequest.cs with [Required] EmployeeId, StartDate, EndDate, Reason, ImprovementAreas, SuccessCriteria
- [X] T140 [P] [US5] Create UpdatePIPRequest DTO in Maliev.PerformanceService.Api/DTOs/UpdatePIPRequest.cs with optional CheckInNote, Status updates
- [X] T141 [P] [US5] Create RecordPIPOutcomeRequest DTO in Maliev.PerformanceService.Api/DTOs/RecordPIPOutcomeRequest.cs with [Required] Outcome (PIPOutcome enum), optional ExtendedEndDate (required if Outcome=ExtendedAgain)
- [X] T142 [US5] Create CreatePIPCommand in Maliev.PerformanceService.Application/Commands/CreatePIPCommand.cs with record struct
- [X] T143 [US5] Create UpdatePIPCommand in Maliev.PerformanceService.Application/Commands/UpdatePIPCommand.cs for check-in notes
- [X] T144 [US5] Create RecordPIPOutcomeCommand in Maliev.PerformanceService.Application/Commands/RecordPIPOutcomeCommand.cs with record struct containing PIPId, Outcome, ExtendedEndDate, RequestingUserId
- [X] T145 [US5] Create GetPIPsQuery in Maliev.PerformanceService.Application/Queries/GetPIPsQuery.cs with record struct containing EmployeeId, RequestingUserId
- [X] T146 [US5] Create CreatePIPValidator in Maliev.PerformanceService.Application/Validators/CreatePIPValidator.cs validating date range (start < end), required fields, HR/manager authorization
- [X] T147 [US5] Create CreatePIPCommandHandler in Maliev.PerformanceService.Application/Handlers/CreatePIPCommandHandler.cs implementing: check for existing active PIP via IPIPRepository.GetActivePIPByEmployeeIdAsync (block creation if found with PIP_ALREADY_ACTIVE error), employee validation, PIP creation with status=Active and ExtensionCount=0, publish PIPInitiatedEvent via MassTransit
- [X] T148 [US5] Create UpdatePIPCommandHandler in Maliev.PerformanceService.Application/Handlers/UpdatePIPCommandHandler.cs implementing: authorization check (HR or manager), append-only check-in notes with timestamp, status updates
- [X] T149 [US5] Create RecordPIPOutcomeCommandHandler in Maliev.PerformanceService.Application/Handlers/RecordPIPOutcomeCommandHandler.cs implementing: authorization check, outcome recording, extension logic (if Outcome=ExtendedAgain: validate ExtensionCount<1, update EndDate, increment ExtensionCount, set status=Extended; if ExtensionCount>=1: block with error MAX_EXTENSION_REACHED), status transition to Completed/Terminated based on outcome, publish PIPCompletedEvent via MassTransit
- [X] T150 [US5] Create GetPIPsQueryHandler in Maliev.PerformanceService.Application/Handlers/GetPIPsQueryHandler.cs implementing authorization (HR sees all, managers see direct reports only) and retrieval
- [X] T151 [US5] Create PIPsController in Maliev.PerformanceService.Api/Controllers/PIPsController.cs with endpoints: POST /performance/v1/employees/{employeeId}/pips (create PIP - HR/admin only), GET /performance/v1/employees/{employeeId}/pips (list PIPs), GET /performance/v1/pips/{pipId} (get single PIP), PUT /performance/v1/pips/{pipId} (update check-in notes), POST /performance/v1/pips/{pipId}/outcome (record outcome), all with [Authorize(Policy = PerformancePermissions.Admin)]
- [X] T152 [US5] Add error handling for PIP_ALREADY_ACTIVE, MAX_EXTENSION_REACHED errors
- [X] T153 [US5] Add structured logging to PIP handlers with PIP ID, employee ID, status changes, outcome

**Checkpoint**: At this point, all user stories (1-6, including 5) should work independently - complete performance management system

---

## Phase 9: Background Services & Reminders

**Purpose**: Automated notifications and reminders for review cycles and PIP check-ins

- [X] T154 Create PerformanceReviewReminderBackgroundService in Maliev.PerformanceService.Infrastructure/BackgroundServices/PerformanceReviewReminderBackgroundService.cs as IHostedService with cron schedule "0 8 * * *" (daily at 8 AM) - query reviews with status=SelfAssessmentPending or ManagerReviewPending, send reminders via INotificationServiceClient, log reminder delivery success/failure
- [X] T155 Create PIPCheckInReminderBackgroundService in Maliev.PerformanceService.Infrastructure/BackgroundServices/PIPCheckInReminderBackgroundService.cs as IHostedService with cron schedule "0 9 * * 1" (weekly on Monday at 9 AM) - query active PIPs, send check-in reminders to managers via INotificationServiceClient, send alerts for PIPs nearing end dates to HR
- [ ] T156 [P] Create unit test for PerformanceReviewReminderBackgroundService in Maliev.PerformanceService.Tests/Unit/BackgroundServices/PerformanceReviewReminderBackgroundServiceTests.cs testing reminder query logic, notification sending
- [ ] T157 [P] Create unit test for PIPCheckInReminderBackgroundService in Maliev.PerformanceService.Tests/Unit/BackgroundServices/PIPCheckInReminderBackgroundServiceTests.cs testing active PIP query, check-in reminders, end date alerts

---

## Phase 10: Event Consumers

**Purpose**: Handle employee lifecycle events from Employee Service

- [X] T158 Create EmployeeCreatedEventConsumer in Maliev.PerformanceService.Infrastructure/Consumers/EmployeeCreatedEventConsumer.cs implementing IConsumer<EmployeeCreatedEvent> - log employee creation event for performance data association, potentially cache employee data in Redis
- [X] T159 Create EmployeeTerminatedEventConsumer in Maliev.PerformanceService.Infrastructure/Consumers/EmployeeTerminatedEventConsumer.cs implementing IConsumer<EmployeeTerminatedEvent> - mark any active reviews as incomplete/closed early, complete or terminate any active PIPs, log termination event
- [ ] T160 [P] Create integration test for EmployeeCreatedEventConsumer in Maliev.PerformanceService.Tests/Integration/Consumers/EmployeeCreatedEventConsumerTests.cs using Testcontainers for RabbitMQ - publish event, verify consumer processes it
- [ ] T161 [P] Create integration test for EmployeeTerminatedEventConsumer in Maliev.PerformanceService.Tests/Integration/Consumers/EmployeeTerminatedEventConsumerTests.cs - verify active reviews closed, PIPs completed on employee termination

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T162 [P] Add OpenAPI/Scalar documentation configuration in Program.cs with API title "Performance Management Service", version "v1", description from spec.md, base path "/performance"
- [X] T163 [P] Add custom metrics in Program.cs using OpenTelemetry Metrics API: performance_reviews_completed_total (counter by rating), performance_goals_completed_total (counter by status), performance_pips_outcomes_total (counter by outcome), performance_notifications_sent_total (counter with success/failure tag)
- [X] T164 [P] Add distributed tracing instrumentation to EmployeeServiceClient and NotificationServiceClient HTTP calls with W3C Trace Context propagation
- [X] T165 [P] Add health check endpoint /performance/liveness returning 200 OK if service is running
- [X] T166 [P] Add readiness check endpoint /performance/readiness returning 200 OK if PostgreSQL, Redis, and RabbitMQ connections are healthy
- [X] T167 Implement data volume limit enforcement: add repository methods to count reviews/goals/feedback per employee, check counts in Create handlers before creation, return DATA_VOLUME_LIMIT_REACHED error with archival guidance when limits reached (50 reviews, 100 goals, 200 feedback)
- [X] T168 Implement soft warning notifications: in Create handlers, check if employee approaching 80% of limits (40 reviews, 80 goals, 160 feedback), send warning to HR via INotificationServiceClient
- [X] T169 Implement archival background service in Maliev.PerformanceService.Infrastructure/BackgroundServices/DataArchivalBackgroundService.cs - query reviews older than 7 years, move to cold storage (Google Cloud Storage), mark as archived in database, log archival actions
- [X] T170 [P] Add security hardening: ensure all PII fields (employee names, IDs) are encrypted in logs, validate no unencrypted PII in error messages, add OWASP top 10 checks (SQL injection prevention via parameterized queries, XSS prevention in API responses, CSRF protection not needed for API-only service)
- [X] T171 [P] Create README.md in repository root with project description, quick start instructions referencing specs/001-performance-service/quickstart.md, link to OpenAPI documentation, CI/CD badge, license information
- [X] T172 Run quickstart.md validation: follow setup steps from specs/001-performance-service/quickstart.md, verify service starts successfully, test sample API calls, confirm all endpoints return expected responses
- [X] T173 [P] Add code comments for complex business logic: review overlap detection, PIP extension logic, anonymity enforcement, data volume limit checks (only where logic is not self-evident)
- [X] T174 Run dotnet build across all projects, ensure zero warnings (TreatWarningsAsErrors=true enforced)
- [X] T175 Run dotnet test with code coverage, verify 80%+ coverage for business-critical logic (handlers, validators, repositories)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - User Story 1 (P1) - Phase 3: Can start after Foundational
  - User Story 2 (P1) - Phase 4: Can start after Foundational (may reference US1 review entity but independently testable)
  - User Story 3 (P2) - Phase 5: Can start after Foundational (independently testable)
  - User Story 4 (P3) - Phase 6: Can start after Foundational (integrates with US1 reviews but independently testable)
  - User Story 6 (P2) - Phase 7: Can start after Foundational (integrates with US2 submission but independently testable)
  - User Story 5 (P4) - Phase 8: Can start after Foundational (independently testable)
- **Background Services (Phase 9)**: Depends on US1, US2, US5 completion for reminders
- **Event Consumers (Phase 10)**: Can start after Foundational (independently testable)
- **Polish (Phase 11)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: FOUNDATIONAL COMPLETE → Can start immediately - No dependencies on other stories
- **User Story 2 (P1)**: FOUNDATIONAL COMPLETE → Can start immediately - References US1 PerformanceReview entity but independently testable (uses same entity)
- **User Story 3 (P2)**: FOUNDATIONAL COMPLETE → Can start immediately - References US1 PerformanceReview for optional linkage but independently testable
- **User Story 4 (P3)**: FOUNDATIONAL COMPLETE → Can start immediately - References US1 PerformanceReview for feedback association but independently testable
- **User Story 6 (P2)**: FOUNDATIONAL COMPLETE → Can start immediately - References US2 submission workflow but independently testable (updates same review entity)
- **User Story 5 (P4)**: FOUNDATIONAL COMPLETE → Can start immediately - Completely independent PIP entity

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD)
- DTOs and Commands/Queries can be created in parallel ([P] marked)
- Validators before handlers (handlers depend on validators)
- Handlers before controllers (controllers depend on handlers)
- Repository implementations before handlers (handlers depend on repositories)
- Event definitions in Domain before handlers (handlers publish events)

### Parallel Opportunities

**Phase 1 (Setup)**: All [P] marked tasks can run in parallel
- T002, T003, T004, T005, T006 (create 5 projects simultaneously)
- T009, T010, T011, T012, T013, T014 (create git/CI files simultaneously)

**Phase 2 (Foundational)**:
- All enum creations T016-T022 can run in parallel
- All entity creations T023-T026 can run in parallel
- All event definitions T027-T033 can run in parallel
- All repository interfaces T035-T038 can run in parallel
- All client interfaces T039-T040 can run in parallel
- All entity configurations T043-T046 can run in parallel
- All repository implementations T049-T052 can run in parallel
- All client implementations T053-T054 can run in parallel

**Phase 3 (User Story 1)**:
- T065, T066, T067 (unit tests) can run in parallel
- T070, T071, T072 (DTOs) can run in parallel
- T073, T074, T075, T076 (Commands/Queries) can run in parallel

**Phase 4-8 (User Stories 2-6, 5)**: Once Foundational phase completes, ALL user stories can start in parallel if team capacity allows
- Different developers can work on US1, US2, US3, US4, US5, US6 simultaneously
- Each story has its own DTOs, commands, handlers, controllers (different files)

**Phase 9-11**: Background services, event consumers, and polish tasks have [P] opportunities for parallel execution

---

## Parallel Example: Foundational Phase

```bash
# Launch all enum definitions together (Phase 2):
Task: "Create ReviewCycle enum in Maliev.PerformanceService.Domain/Enums/ReviewCycle.cs"
Task: "Create PerformanceRating enum in Maliev.PerformanceService.Domain/Enums/PerformanceRating.cs"
Task: "Create ReviewStatus enum in Maliev.PerformanceService.Domain/Enums/ReviewStatus.cs"
Task: "Create GoalStatus enum in Maliev.PerformanceService.Domain/Enums/GoalStatus.cs"
Task: "Create PIPStatus enum in Maliev.PerformanceService.Domain/Enums/PIPStatus.cs"
Task: "Create PIPOutcome enum in Maliev.PerformanceService.Domain/Enums/PIPOutcome.cs"
Task: "Create FeedbackType enum in Maliev.PerformanceService.Domain/Enums/FeedbackType.cs"

# Launch all entity definitions together (Phase 2):
Task: "Create PerformanceReview entity in Maliev.PerformanceService.Domain/Entities/PerformanceReview.cs"
Task: "Create Goal entity in Maliev.PerformanceService.Domain/Entities/Goal.cs"
Task: "Create ReviewFeedback entity in Maliev.PerformanceService.Domain/Entities/ReviewFeedback.cs"
Task: "Create PerformanceImprovementPlan entity in Maliev.PerformanceService.Domain/Entities/PerformanceImprovementPlan.cs"
```

---

## Parallel Example: User Story 1

```bash
# Launch all unit tests for User Story 1 together:
Task: "Create unit test for CreatePerformanceReviewValidator in Maliev.PerformanceService.Tests/Unit/Validators/CreatePerformanceReviewValidatorTests.cs"
Task: "Create unit test for CreatePerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/CreatePerformanceReviewCommandHandlerTests.cs"
Task: "Create unit test for UpdatePerformanceReviewCommandHandler in Maliev.PerformanceService.Tests/Unit/Handlers/UpdatePerformanceReviewCommandHandlerTests.cs"

# Launch all DTOs for User Story 1 together:
Task: "Create PerformanceReviewDto in Maliev.PerformanceService.Api/DTOs/PerformanceReviewDto.cs"
Task: "Create CreatePerformanceReviewRequest DTO in Maliev.PerformanceService.Api/DTOs/CreatePerformanceReviewRequest.cs"
Task: "Create UpdatePerformanceReviewRequest DTO in Maliev.PerformanceService.Api/DTOs/UpdatePerformanceReviewRequest.cs"

# Launch all Commands/Queries for User Story 1 together:
Task: "Create CreatePerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/CreatePerformanceReviewCommand.cs"
Task: "Create UpdatePerformanceReviewCommand in Maliev.PerformanceService.Application/Commands/UpdatePerformanceReviewCommand.cs"
Task: "Create GetPerformanceReviewsQuery in Maliev.PerformanceService.Application/Queries/GetPerformanceReviewsQuery.cs"
Task: "Create GetPerformanceReviewByIdQuery in Maliev.PerformanceService.Application/Queries/GetPerformanceReviewByIdQuery.cs"
```

---

## Parallel Example: All User Stories After Foundational

```bash
# Once Foundational (Phase 2) is complete, launch all user stories in parallel with multiple developers:

# Developer A: User Story 1 (Phase 3)
Task: "T065-T085 User Story 1 implementation"

# Developer B: User Story 2 (Phase 4)
Task: "T086-T093 User Story 2 implementation"

# Developer C: User Story 3 (Phase 5)
Task: "T094-T115 User Story 3 implementation"

# Developer D: User Story 4 (Phase 6)
Task: "T116-T127 User Story 4 implementation"

# Developer E: User Story 6 (Phase 7)
Task: "T128-T133 User Story 6 implementation"

# Developer F: User Story 5 (Phase 8)
Task: "T134-T153 User Story 5 implementation"
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 Only - P1 Priority)

1. Complete Phase 1: Setup (T001-T015)
2. Complete Phase 2: Foundational (T016-T064) - CRITICAL, blocks all stories
3. **CHECKPOINT**: Foundation ready - can begin user story work
4. Complete Phase 3: User Story 1 - Employee Self-Review Submission (T065-T085)
5. **STOP and VALIDATE**: Test User Story 1 independently - employees can create and submit self-assessments
6. Complete Phase 4: User Story 2 - Manager Performance Review and Rating (T086-T093)
7. **STOP and VALIDATE**: Test User Stories 1 AND 2 together - complete review lifecycle from employee to manager
8. **MVP READY**: Deploy/demo basic performance review capability

### Incremental Delivery (Add User Stories by Priority)

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 (P1) → Test independently → Deploy/Demo
3. Add User Story 2 (P1) → Test independently → Deploy/Demo (MVP!)
4. Add User Story 3 (P2) → Test independently → Deploy/Demo (goal tracking added)
5. Add User Story 6 (P2) → Test independently → Deploy/Demo (acknowledgment workflow complete)
6. Add User Story 4 (P3) → Test independently → Deploy/Demo (360-degree feedback enrichment)
7. Add User Story 5 (P4) → Test independently → Deploy/Demo (PIP management for underperformers)
8. Add Background Services (Phase 9) → Automated reminders active
9. Add Event Consumers (Phase 10) → Employee lifecycle integration
10. Complete Polish (Phase 11) → Production-ready with observability, archival, limits

Each story adds value without breaking previous stories.

### Parallel Team Strategy (6 Developers)

With multiple developers after Foundational phase completes:

1. **All Team**: Complete Setup (Phase 1) together - 15 tasks
2. **All Team**: Complete Foundational (Phase 2) together - 49 tasks (many [P] opportunities)
3. **CHECKPOINT**: Foundation ready - split team by user story priority
4. **Developer A**: User Story 1 (P1) - 21 tasks
5. **Developer B**: User Story 2 (P1) - 8 tasks (finishes early, helps others)
6. **Developer C**: User Story 3 (P2) - 22 tasks
7. **Developer D**: User Story 4 (P3) - 12 tasks
8. **Developer E**: User Story 6 (P2) - 6 tasks (finishes early, helps others)
9. **Developer F**: User Story 5 (P4) - 20 tasks
10. **All Team**: Integrate stories, complete Background Services (Phase 9), Event Consumers (Phase 10), Polish (Phase 11)

Stories complete and integrate independently, then combine for full system.

---

## Notes

- [P] tasks = different files, no dependencies, can execute in parallel
- [Story] label maps task to specific user story for traceability (US1-US6)
- Each user story is independently completable and testable
- Tests are included for all user stories (TDD approach)
- Verify tests fail before implementing (red-green-refactor)
- Commit after each task or logical group
- Stop at checkpoints to validate story independently
- Foundational phase MUST be complete before ANY user story work begins
- User stories can proceed in parallel after Foundational phase (if team capacity allows)
- Sequential priority order: P1 (US1, US2) → P2 (US3, US6) → P3 (US4) → P4 (US5)
- No AutoMapper, FluentValidation, or FluentAssertions per constitution
- All tasks include exact file paths for clarity
- Background services depend on specific user stories (Phase 9 after US1, US2, US5)
- Event consumers are independent (Phase 10 can run anytime after Foundational)
- Polish phase (Phase 11) should be done after all desired user stories are complete

**Total Tasks**: 175 tasks covering complete Performance Management Service implementation from setup through production-ready deployment
