# Maliev.PerformanceService Agent Guidelines

This document provides instructions for AI agents working on the Maliev.PerformanceService codebase.
Follow these guidelines to ensure consistency, quality, and adherence to project standards.

## 1. Environment & Commands

- **Platform**: .NET 10.0 (C# 13) / ASP.NET Core 10.0
- **Solution File**: `Maliev.PerformanceService.slnx`
- **Database**: PostgreSQL 18 (Entities), Redis 7.x (Cache)
- **Messaging**: RabbitMQ (MassTransit)

### Build & Test
- **Build Solution**:
  ```bash
  dotnet build
  ```
- **Run All Tests**:
  ```bash
  dotnet test
  ```
- **Run Single Test**:
  Use the `FullyQualifiedName` filter.
  ```bash
  dotnet test --filter "FullyQualifiedName~Maliev.PerformanceService.Tests.Unit.Handlers.CreateGoalCommandHandlerTests.HandleAsync_ValidCommand_CreatesGoal"
  ```
- **Format Code**:
  ```bash
  dotnet format
  ```

## 2. Project Structure

- **Api** (`Maliev.PerformanceService.Api`): REST API Controllers, DTOs, Program.cs.
- **Application** (`Maliev.PerformanceService.Application`): Core business logic, CQRS (Commands, Queries, Handlers), Validators, Interfaces.
- **Domain** (`Maliev.PerformanceService.Domain`): Entities, Enums, Domain Events.
- **Infrastructure** (`Maliev.PerformanceService.Infrastructure`): EF Core Context, Repositories, External Services implementations, Background Services.
- **Tests** (`Maliev.PerformanceService.Tests`):
  - `Unit/`: Logic tests using Moq and xUnit.
  - `Integration/`: End-to-end tests using Testcontainers (Real Postgres/Redis/RabbitMQ).

## 3. Code Style & Conventions

### General
- **Formatting**: Follow standard .NET conventions.
- **Namespaces**: `Maliev.PerformanceService.{Layer}.{Feature}`.
- **Classes**: PascalCase.
- **Interfaces**: PascalCase with `I` prefix (e.g., `IGoalRepository`).
- **Async**: Use `async/await` for all I/O bound operations. Append `Async` to method names (e.g., `CreateAsync`).
- **Documentation**: Use XML comments (`/// <summary>`) for all public members, classes, and interfaces.

### CQRS Pattern
- **Commands/Queries**: Use `record` types for immutability.
- **Handlers**:
  - Implement `HandleAsync` methods.
  - **Return Pattern**: Use `Task<(ResultType? Result, string? Error)>` tuple for operation outcomes.
  - **Do not throw exceptions** for known domain/validation errors; return the error string in the tuple.
  - Inject dependencies via constructor (DI).

### Validation
- **Validators**: Use custom validator classes (e.g., `CreateGoalValidator`).
- **Return Type**: `(bool IsValid, string? Error)`.
- **Constraint**: **NO FluentValidation**. Use manual checks or Data Annotations where appropriate.

### Domain Modeling
- **Entities**: Located in Domain layer.
- **Repositories**: Define interfaces in Application, implement in Infrastructure.
- **IDs**: Use `Guid` for entity identifiers.

### Error Handling & Logging
- **Logging**: Inject `ILogger<T>`. Use structured logging (e.g., `_logger.LogInformation("Creating goal for {EmployeeId}", command.EmployeeId)`).
- **Exceptions**: Reserve exceptions for unexpected system failures. Domain rules should be handled via the result tuple pattern.

## 4. Testing Guidelines

- **Framework**: xUnit.
- **Assertions**: Use xUnit `Assert` (e.g., `Assert.NotNull`, `Assert.Equal`). **NO FluentAssertions**.
- **Unit Tests**:
  - Mock dependencies using `Moq`.
  - Locate in `Tests/Unit`.
- **Integration Tests**:
  - Use `Testcontainers` for database/messaging.
  - Do not mock the DbContext; use the real containerized database.
  - Locate in `Tests/Integration`.

## 5. Critical Rules (from CLAUDE.md)

1.  **NO AutoMapper**: Map properties explicitly.
2.  **NO FluentValidation**: Use manual validation or Data Annotations.
3.  **NO FluentAssertions**: Use standard `Assert`.
4.  **Testcontainers**: Mandatory for integration tests.
5.  **Project Structure**: Keep the flat structure (no `/src` or `/tests` root folders).

## 6. Development Workflow for Agents

1.  **Read**: Always read relevant files (`CLAUDE.md`, related code) before starting.
2.  **Plan**: Analyze the task and existing patterns (CQRS, error handling).
3.  **Implement**: Write code adhering to the styles above.
4.  **Verify**:
    - Run `dotnet build`.
    - Run `dotnet test`.
    - If modifying logic, add or update tests.

## 7. Example: Creating a Handler

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


## Database & EF Core — Mandatory Rules

### EF Core Design Package
- ❌ `Microsoft.EntityFrameworkCore.Design` MUST NOT be in Api projects
- ✅ It belongs ONLY in the Infrastructure (or Data) project where migrations live
- Migration commands must target Infrastructure, not Api:
  ```
  dotnet ef migrations add <Name> --project Maliev.<Domain>Service.Infrastructure --startup-project ../Maliev.<Domain>Service.Api
  ```

### PostgreSQL xmin Concurrency — Mandatory Pattern
Use shadow property ONLY. Never add a Xmin/xmin property to domain entities.
```csharp
entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
```
- ❌ Never use `UseXminAsConcurrencyToken()` (removed in Npgsql EF v7)
- ❌ Never use entity property `public uint Xmin { get; set; }` or `public uint xmin { get; set; }`
- ❌ Never use `.Ignore(e => e.Xmin)` — remove the entity property instead
