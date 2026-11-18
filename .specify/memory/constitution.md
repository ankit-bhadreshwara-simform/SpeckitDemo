<!--
SYNC IMPACT REPORT
==================
Version change: 1.1.0 -> 2.0.0
Modified principles:
  - I. Project Structure & Separation of Concerns
    Project names now match the SpeckitDemo solution:
    SpeckitDemo.Api, SpeckitDemo.Application, SpeckitDemo.Contracts, SpeckitDemo.Data,
    SpeckitDemo.DataAccess, SpeckitDemo.Security, SpeckitDemo.Tests
Added sections: None
Removed sections: None (no principles removed; Principle I redefined)
Templates requiring updates:
  - .specify/templates/plan-template.md  ✅ updated — project structure tree updated
  - .specify/templates/spec-template.md  ✅ no changes required
  - .specify/templates/tasks-template.md ✅ updated — path conventions and phase structure updated
Deferred TODOs: None
-->

# SpeckitDemo Constitution

## Core Principles

### I. Project Structure & Separation of Concerns (NON-NEGOTIABLE)

The solution MUST be organized into seven independently compilable projects.
Each project MUST have a single, well-defined responsibility. Dependency
references MUST flow strictly according to the rules below; no circular or
upward violations are permitted.

**Mandatory project layout and dependency flow:**

- **SpeckitDemo.Contracts** — inbound payloads, outbound profiles, and
  `ResultEnvelope<T>`; no project references.
- **SpeckitDemo.Data** — `SpeckitDemoContext`, `Member` mapping, migrations, and
  the SQLite setup; no project references.
- **SpeckitDemo.DataAccess** — account query abstractions and stores; references
  `SpeckitDemo.Data`.
- **SpeckitDemo.Application** — sign-in and profile workflows plus validation;
  references `SpeckitDemo.Contracts`, `SpeckitDemo.DataAccess`, and
  `SpeckitDemo.Security`.
- **SpeckitDemo.Security** — token signing and password hashing; no project
  references and no EF Core dependency.
- **SpeckitDemo.Api** — HTTP host: endpoints, pipeline, and DI wiring in
  `Program.cs`; references `SpeckitDemo.Application`, `SpeckitDemo.Contracts`,
  `SpeckitDemo.Data`, `SpeckitDemo.DataAccess`, and `SpeckitDemo.Security`.
- **SpeckitDemo.Tests** — xUnit + Moq; references `SpeckitDemo.Application`,
  `SpeckitDemo.Contracts`, `SpeckitDemo.Data`, `SpeckitDemo.DataAccess`, and
  `SpeckitDemo.Security`. MUST NOT reference `SpeckitDemo.Api`.

**Dependency reference matrix (✅ allowed / ❌ prohibited):**

| From \ To | Contracts | Data | DataAccess | Application | Security | Api |
|---|---|---|---|---|---|---|
| SpeckitDemo.Data | ❌ | — | ❌ | ❌ | ❌ | ❌ |
| SpeckitDemo.DataAccess | ❌ | ✅ | — | ❌ | ❌ | ❌ |
| SpeckitDemo.Security | ❌ | ❌ | ❌ | ❌ | — | ❌ |
| SpeckitDemo.Application | ✅ | ❌ | ✅ | — | ✅ | ❌ |
| SpeckitDemo.Api | ✅ | ✅ | ✅ | ✅ | ✅ | — |
| SpeckitDemo.Tests | ✅ | ✅ | ✅ | ✅ | ✅ | ❌ |

**Rules:**
- Endpoints in `SpeckitDemo.Api` MUST NOT contain business logic; delegate to
  workflows in `SpeckitDemo.Application`.
- Endpoint classes MUST NOT use `SpeckitDemo.Data` or `SpeckitDemo.DataAccess`
  types. Those references exist so the host can register them.
- Domain entities live in `SpeckitDemo.Data.Members`. API contracts live in
  `SpeckitDemo.Contracts` and MUST NOT expose `Member`.
- `SpeckitDemo.Security` MUST remain stateless and free of EF Core dependencies.

### II. SOLID Principles (NON-NEGOTIABLE)

All production code MUST conform to the five SOLID principles:

- **Single Responsibility**: Every class/method MUST have exactly one reason to
  change. Classes exceeding ~200 LOC warrant a mandatory review.
- **Open/Closed**: Behavior extension MUST be achieved via abstraction
  (interfaces, inheritance) without modifying existing code.
- **Liskov Substitution**: Subtypes and interface implementations MUST be fully
  substitutable for their base types without altering program correctness.
- **Interface Segregation**: Interfaces MUST be narrow and role-specific.
  Fat interfaces with unrelated methods are prohibited.
- **Dependency Inversion**: High-level modules MUST depend on abstractions, not
  concretions. All dependencies MUST be injected through constructors.

### III. Repository Pattern & Service Layer (NON-NEGOTIABLE)

- Every data-access concern MUST be encapsulated behind a repository interface
  defined in the Application layer (e.g., `IAccountReader`).
- Repository implementations MUST reside in the Infrastructure layer and MUST
  NOT be referenced directly outside of DI registration.
- A service layer (Application services) MUST mediate between controllers and
  repositories. Controllers MUST NOT call repositories directly.
- Unit of Work pattern SHOULD be used when multiple repositories participate in
  a single transaction.

### IV. API Standards & RESTful Conventions (NON-NEGOTIABLE)

- All public endpoints MUST follow RESTful resource naming:
  `GET /api/v{version}/resources`, `POST /api/v{version}/resources`, etc.
- API versioning MUST be implemented via URL path prefix (`/api/v1/`, `/api/v2/`).
  Breaking changes MUST increment the major version.
- Request payloads MUST be received as typed Request DTOs; response payloads
  MUST be returned as typed Response DTOs. Domain entities MUST NOT be exposed
  directly in API responses.
- All endpoints MUST return a standard `ResultEnvelope<T>` envelope:
  `{ "success": bool, "data": T, "message": string, "errors": string[] }`.
- HTTP status codes MUST be semantically correct: `200 OK`, `201 Created`,
  `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`,
  `422 Unprocessable Entity`, `500 Internal Server Error`.
- `ResultEnvelope<T>` factory methods (`Ok`, `Fail`) MUST be used to construct
  all responses consistently. The class MUST be defined in
  `SpeckitDemo.Contracts/Envelope/ResultEnvelope.cs`.

### V. Global Exception Handling & Structured Logging (NON-NEGOTIABLE)

- A single global exception-handling middleware MUST catch all unhandled
  exceptions and return a consistent `ResultEnvelope` error envelope.
  Controllers MUST NOT contain try/catch blocks for unhandled exceptions.
- All logging MUST use `ILogger<T>` from `Microsoft.Extensions.Logging`.
  `Console.WriteLine`, `Debug.WriteLine`, and third-party loggers injected
  directly into application code are prohibited.
- Log levels MUST be used correctly:
  - `LogInformation` - normal business flow events
  - `LogWarning` - recoverable anomalies, input validation failures
  - `LogError` - unexpected exceptions caught by middleware
  - `LogDebug` - diagnostic detail; disabled in production
- Sensitive data (passwords, tokens, PII) MUST NOT appear in log output.

### VI. Entity Framework Core Code-First with SQLite (NON-NEGOTIABLE)

- The database schema MUST be managed exclusively via EF Core Code-First
  migrations. Direct SQL DDL scripts are prohibited.
- `DbContext` MUST reside in the Infrastructure layer. The Application layer
  references only repository interfaces.
- All entity configurations MUST use the Fluent API in separate
  `IEntityTypeConfiguration<T>` classes. Data Annotations on domain entities
  are prohibited.
- Migrations MUST be committed to source control. The project MUST NOT apply
  migrations automatically at startup in production environments.
- SQLite is the default database for development and testing. The connection
  string MUST be configurable via `appsettings.json` / environment variables.

### VII. Dependency Injection & Interface-Based Design (NON-NEGOTIABLE)

- All service, repository, and infrastructure dependencies MUST be registered
  in the DI container and injected via constructor injection.
- Property injection and service locator patterns (`IServiceProvider` resolved
  inline) are prohibited outside of the composition root.
- Lifetime rules:
  - `Scoped` - repositories, `DbContext`, application services (default)
  - `Singleton` - stateless infrastructure utilities (e.g., token generators)
  - `Transient` - lightweight, stateless helpers with no shared state
- Every class participating in DI MUST program against an interface.
  Concrete types MUST NOT be injected directly except for framework primitives.

### VIII. Coding Standards & Naming Conventions (NON-NEGOTIABLE)

**General:**
- Target framework: **.NET 10**, C# latest language version.
- Nullable reference types MUST be enabled (`<Nullable>enable</Nullable>`).
- Implicit usings MUST be enabled (`<ImplicitUsings>enable</ImplicitUsings>`).
- All public members MUST have XML documentation comments (`/// <summary>`).

**Naming conventions:**

| Construct | Convention | Example |
|---|---|---|
| Namespace | PascalCase, mirrors project + folder path | `SpeckitDemo.Application.Workflows` |
| Class / Interface | PascalCase; interfaces prefixed `I` | `IAccountReader`, `SignInWorkflow` |
| Method | PascalCase | `FindByIdAsync` |
| Property | PascalCase | `FirstName` |
| Private field | camelCase with `_` prefix | `_accountReader` |
| Local variable | camelCase | `userId` |
| Constant | PascalCase | `MaxRetryCount` |
| Enum | PascalCase (type and members) | `UserRole.Admin` |
| Contract types | Payload / profile / envelope | `CredentialPayload`, `AccountProfile`, `ResultEnvelope<T>` |
| Controller suffix | `Controller` | `SignInController`, `AccountController` |
| Data-access suffix | `Reader` | `AccountReader` |
| Workflow suffix | `Workflow` | `SignInWorkflow` |

**File organization:** One public type per file; filename MUST match the type name.

### IX. Unit Testing Standards (NON-NEGOTIABLE)

- Unit tests MUST be written using **xUnit** as the test framework.
- Mocking MUST be performed with **Moq**. No other mocking libraries are permitted.
- Test project: `SpeckitDemo.Tests` (placed under `tests/` at solution root).
- **Write implementation code first, then write unit tests immediately after**
  for each class before moving to the next feature.
- Every Application service method MUST have at least one passing and one
  failing scenario covered by unit tests.
- Test method naming convention: `MethodName_StateUnderTest_ExpectedBehaviour`
  (e.g., `GetUserById_UserNotFound_ThrowsNotFoundException`).
- Tests MUST NOT touch the database, file system, or network. All data-access
  concerns MUST be mocked via repository interfaces.
- Arrange/Act/Assert (AAA) pattern MUST be used in every test method.

### X. Security Baseline & Input Validation (NON-NEGOTIABLE)

- All incoming request DTOs MUST be validated using **FluentValidation** before
  reaching the service layer. Unvalidated input MUST NOT be processed.
- JWT authentication MUST use `Microsoft.AspNetCore.Authentication.JwtBearer`.
  Tokens MUST be validated for signature, expiry, issuer, and audience.
- Secrets (JWT signing keys, connection strings) MUST be stored in
  `appsettings.Development.json` (dev only, git-ignored) or environment
  variables / secrets manager in production. Plain-text secrets in committed
  files are prohibited.
- HTTPS MUST be enforced in all non-development environments via
  `UseHttpsRedirection()` middleware.
- CORS policy MUST be explicitly configured; wildcard `*` origins are
  prohibited in production environments.
- Passwords MUST be hashed using `ISecretHasher`. Plain-text password
  storage is strictly prohibited.
- All database queries MUST be parameterized (EF Core by default; raw SQL via
  `FromSqlRaw` or `ExecuteSqlRaw` MUST use parameterized overloads).

## Technology Stack

| Concern | Technology | Version |
|---|---|---|
| Framework | ASP.NET Core Web API | .NET 10 |
| Language | C# | Latest (C# 13+) |
| ORM | Entity Framework Core | 9.x (EF Core for .NET 10) |
| Database | SQLite | 3.x |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`) | .NET 10 built-in |
| Validation | FluentValidation | Latest stable |
| Unit Testing | xUnit | Latest stable |
| Mocking | Moq | Latest stable |
| Logging | Microsoft.Extensions.Logging (`ILogger<T>`) | .NET 10 built-in |
| API Documentation | Scalar / Swashbuckle (OpenAPI 3.x) | Latest stable |
| API Versioning | Asp.Versioning.Mvc | Latest stable |

## Project Structure & Folder Organization

```text
SpeckitDemo/
+-- src/
|   +-- SpeckitDemo.Contracts/              # Inbound payloads, outbound profiles, envelopes
|   |   +-- Inbound/
|   |   |   +-- CredentialPayload.cs
|   |   +-- Outbound/
|   |   |   +-- IssuedToken.cs
|   |   |   +-- AccountProfile.cs
|   |   +-- Envelope/
|   |       +-- ResultEnvelope.cs
|   |
|   +-- SpeckitDemo.Data/                   # EF Core context, member mapping, migrations
|   |   +-- SpeckitDemoContext.cs
|   |   +-- Mapping/
|   |   |   +-- MemberMap.cs
|   |   +-- Members/
|   |   |   +-- Member.cs
|   |   +-- Migrations/
|   |   +-- Design/
|   |
|   +-- SpeckitDemo.DataAccess/             # Account query abstractions and stores
|   |   +-- Abstractions/
|   |   |   +-- IAccountReader.cs
|   |   +-- Stores/
|   |       +-- AccountReader.cs
|   |
|   +-- SpeckitDemo.Application/            # Sign-in and profile workflows
|   |   +-- Abstractions/
|   |   |   +-- ISignInWorkflow.cs
|   |   |   +-- IAccountLookup.cs
|   |   +-- Workflows/
|   |   |   +-- SignInWorkflow.cs
|   |   |   +-- AccountLookup.cs
|   |   +-- Validation/
|   |       +-- CredentialRules.cs
|   |
|   +-- SpeckitDemo.Security/               # Token signing and password hashing
|   |   +-- Abstractions/
|   |   |   +-- ITokenIssuer.cs
|   |   |   +-- ISecretHasher.cs
|   |   +-- Tokens/
|   |   |   +-- TokenIssuer.cs
|   |   +-- Passwords/
|   |       +-- SecretHasher.cs
|   |
|   +-- SpeckitDemo.Api/                    # HTTP host: endpoints, pipeline, Program.cs
|       +-- Endpoints/
|       |   +-- v1/
|       |       +-- SignInController.cs
|       |       +-- AccountController.cs
|       +-- Pipeline/
|       |   +-- UnhandledErrorMiddleware.cs
|       +-- Hosting/
|       +-- appsettings.json
|       +-- appsettings.Development.json    # Git-ignored; local secrets only
|       +-- Program.cs
|
+-- tests/
    +-- SpeckitDemo.Tests/                  # xUnit + Moq; no DB/HTTP access
        +-- Workflows/
        +-- Validation/
```

**Rules:**
- Projects MUST NOT add project references that violate the dependency matrix
  in Principle I. Any deviation MUST be documented in the plan's Complexity
  Tracking table with explicit justification.
- All shared cross-cutting response models (e.g., `ResultEnvelope<T>`) belong in
  `SpeckitDemo.Contracts/Envelope/`, NOT in `SpeckitDemo.Api`.
- `appsettings.Development.json` MUST be listed in `.gitignore`.

## API Standards

### Standard Response Envelope

All API responses MUST conform to the following structure:

```csharp
public class ResultEnvelope<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = [];

    public static ResultEnvelope<T> Ok(T data, string message = "") =>
        new() { Success = true, Data = data, Message = message };

    public static ResultEnvelope<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? [] };
}
```

### Versioning

- URL path versioning: `/api/v1/`, `/api/v2/`
- Use `Asp.Versioning.Mvc` package for version negotiation.
- Default version: `v1`. Version MUST be explicit in all route attributes.

### Controller Conventions

- Controllers MUST inherit from `ControllerBase` (not `Controller`).
- Controllers MUST be decorated with `[ApiController]` and
  `[Route("api/v{version:apiVersion}/[controller]")]`.
- Action methods MUST use specific HTTP verb attributes with explicit route
  templates where needed.
- Async/await MUST be used for all I/O-bound operations; synchronous blocking
  I/O is prohibited in controllers and services.

## Testing Standards

- Test projects MUST be placed under `tests/` at the solution root.
- Unit tests (`SpeckitDemo.Tests`) cover services and validators (no DB, no HTTP).
- Integration tests (optional) cover API endpoints against a test SQLite
  database using `WebApplicationFactory<Program>` — place in a separate
  `SpeckitDemo.IntegrationTests` project if needed.
- Minimum coverage expectation for `SpeckitDemo.Application`: **80% line coverage**
  (enforced via CI gate once CI is established).
- Test data MUST use builder or object-mother patterns to avoid brittle,
  copy-pasted setup code.
- Each unit test class MUST focus on a single production class.

## Security & Validation Standards

- Request validation MUST occur in the Application layer via FluentValidation
  validators invoked before service logic executes.
- JWT tokens MUST specify: `Issuer`, `Audience`, `Expiry`, `SigningKey`
  (HS256 minimum; RS256 preferred for production).
- Refresh token rotation MUST be implemented if long-lived sessions are
  required; single-use refresh tokens only.
- All database queries MUST be parameterized (EF Core handles this by default).
- Passwords MUST be hashed using `ISecretHasher`.

## Governance

This constitution supersedes all other development practices, team conventions,
and informal agreements for the SpeckitDemo solution. It applies to all
contributors, human and AI-assisted alike.

**Amendment Procedure:**
1. Propose the amendment in a pull request that modifies this file.
2. Increment `CONSTITUTION_VERSION` following semantic versioning rules
   (MAJOR / MINOR / PATCH as defined in the versioning policy below).
3. Update `LAST_AMENDED_DATE` to the ISO date of the amendment.
4. Run the `/speckit.constitution` command to propagate changes to all
   dependent templates and generate a new Sync Impact Report.
5. The amended constitution takes effect upon merge to the default branch.

**Versioning Policy:**
- MAJOR - removal or redefinition of a non-negotiable principle; breaking
  change to project structure or naming conventions.
- MINOR - addition of a new principle, section, or materially expanded guidance.
- PATCH - clarifications, wording improvements, typo fixes, non-semantic
  refinements.

**Compliance Review:**
- Every pull request MUST include a Constitution Check confirming no principles
  are violated. The plan template's Constitution Check section provides the
  gate list.
- Violations that are intentionally deferred MUST be documented in the
  Complexity Tracking table of the relevant plan file with explicit justification.
- AI-assisted generation MUST be guided by this constitution. Generated code
  that violates a non-negotiable principle MUST be rejected or refactored before
  merge.

**Version**: 2.0.0 | **Ratified**: 2026-04-03 | **Last Amended**: 2026-11-18
