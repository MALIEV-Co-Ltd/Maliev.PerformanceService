# Maliev.PerformanceService Agent Guidelines

This document provides instructions for AI agents working on the Maliev.PerformanceService codebase.
Follow these guidelines to ensure consistency, quality, and adherence to project standards.

## 1. Environment & Commands

- **Platform**: .NET 10.0 (C# 13) / ASP.NET Core 10.0
- **Solution File**: `Maliev.PerformanceService.slnx`
- **Database**: PostgreSQL 18 (Entities), Redis 7.x (Cache)
- **Messaging**: RabbitMQ (MassTransit)

### Build & Test

All commands run from within the service directory (`B:\maliev\Maliev.PerformanceService`).

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.PerformanceService.slnx

# Run all tests
dotnet test Maliev.PerformanceService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~CreateGoalCommandHandlerTests.HandleAsync_ValidCommand_CreatesGoal"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~CreateGoalCommandHandlerTests"

# Run with code coverage
dotnet test Maliev.PerformanceService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.PerformanceService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.PerformanceService.Infrastructure --startup-project Maliev.PerformanceService.Infrastructure
```

## 2. Project Structure

```
Maliev.PerformanceService/
├── Maliev.PerformanceService.Api/           # Controllers, Consumers, Middleware
├── Maliev.PerformanceService.Application/   # Use cases, DTOs, Interfaces, Handlers
├── Maliev.PerformanceService.Domain/        # Entities, value objects, domain interfaces
├── Maliev.PerformanceService.Infrastructure/ # EF Core DbContext, repositories, HTTP clients
├── Maliev.PerformanceService.Tests/         # Unit + Integration tests (xUnit)
│   ├── Unit/                                # Logic tests using Moq and xUnit
│   └── Integration/                         # E2E tests using Testcontainers (Postgres/Redis/RabbitMQ)
├── Directory.Build.props                    # Central package versioning
└── Maliev.PerformanceService.slnx           # Solution file (.slnx preferred over .sln)
```

## 3. Code Style & Conventions

### C# Naming & Formatting
- **Namespaces**: File-scoped (`namespace Maliev.PerformanceService.Domain.Entities;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `CreateAsync`)
- **Interfaces**: Prefix with `I` (e.g., `IGoalRepository`)
- **Permissions**: GCP-style `{domain}.{plural-resource}.{action}` as `public const string` in a `Permissions` static class
  - Valid: `performance.goals.create`, `performance.reviews.submit`
  - Invalid: `performance.goal.create` (singular), `performance.create` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace

### C# Patterns
- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("performance/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Creating goal for {EmployeeId}", employeeId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Check existing conventions in this service for naming policy
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned

### CQRS Pattern (Service-Specific)
- **Commands/Queries**: Use `record` types for immutability.
- **Handlers**:
  - Implement `HandleAsync` methods.
  - **Return Pattern**: Use `Task<(ResultType? Result, string? Error)>` tuple for operation outcomes.
  - **Do not throw exceptions** for known domain/validation errors; return the error string in the tuple.
  - Inject dependencies via constructor (DI).

### Validation (Service-Specific)
- **Validators**: Use custom validator classes (e.g., `CreateGoalValidator`).
- **Return Type**: `(bool IsValid, string? Error)`.
- **Constraint**: **NO FluentValidation**. Use manual checks or Data Annotations where appropriate.

### Domain Modeling (Service-Specific)
- **Entities**: Located in Domain layer.
- **Repositories**: Define interfaces in Application, implement in Infrastructure.
- **IDs**: Use `Guid` for entity identifiers.

---

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/performance/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

---

## Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`
- **Unit Tests**: Mock dependencies using `Moq`. Locate in `Tests/Unit`.
- **Integration Tests**: Do not mock the DbContext; use the real containerized database. Locate in `Tests/Integration`.

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

#### Key Rules
- Use `BaseIntegrationTestFactory<TProgram, TDbContext>` for integration tests (real Testcontainers, never InMemoryDatabase)
- Test naming: `MethodName_StateUnderTest_ExpectedBehavior`
- Minimum 80% code coverage
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

---

## Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("performance.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/performance`
- **Scalar docs**: Configured at `/performance/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards
- **Project Structure**: Keep the flat structure (no `/src` or `/tests` root folders)

---

## Example: Creating a Handler

```csharp
public class CreateItemCommandHandler
{
    private readonly IRepository _repository;
    private readonly CreateItemValidator _validator;

    public CreateItemCommandHandler(IRepository repository, CreateItemValidator validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<(Item? Item, string? Error)> HandleAsync(CreateItemCommand command, CancellationToken cancellationToken)
    {
        var (isValid, error) = _validator.Validate(command);
        if (!isValid) return (null, error);

        var item = new Item { ... };
        await _repository.CreateAsync(item, cancellationToken);
        return (item, null);
    }
}
```

---

## Git Rules

- Each `Maliev.*` folder is an independent git repo. `cd` into it before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked

---

## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure as both project and startup-project (since EF Core Design package is in Infrastructure):
  ```
  dotnet ef migrations add <Name> --project Maliev.PerformanceService.Infrastructure --startup-project Maliev.PerformanceService.Infrastructure
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead

---

## Development Workflow for Agents

1.  **Read**: Always read relevant files (`AGENTS.md`, related code) before starting.
2.  **Plan**: Analyze the task and existing patterns (CQRS, error handling).
3.  **Implement**: Write code adhering to the styles above.
4.  **Verify**:
    - Run `dotnet build Maliev.PerformanceService.slnx`.
    - Run `dotnet test Maliev.PerformanceService.slnx --verbosity normal`.
    - If modifying logic, add or update tests.
