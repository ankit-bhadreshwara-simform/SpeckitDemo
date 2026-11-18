# Implementation Plan: JWT Authentication API

**Branch**: `002-jwt-auth-api` | **Date**: 2026-04-03 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `/specs/002-jwt-auth-api/spec.md`

## Summary

Implement a stateless JWT authentication API on .NET 10 / ASP.NET Core Web API. The system exposes two endpoints: a public `POST /api/v1/auth/login` that validates username/password against a BCrypt-hashed SQLite database and returns a signed HS256 Bearer token, and a protected `GET /api/v1/users/me` that validates the token and re-checks user existence on every call. The solution follows the seven-project SpeckitDemo layout (Contracts / Data / DataAccess / Application / Security / Api / Tests) with full FluentValidation, global exception middleware, structured JSON console logging, and xUnit + Moq unit tests.

## Technical Context

**Language/Version**: .NET 10 / C# 13+  
**Primary Dependencies**:
- `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.5
- `System.IdentityModel.Tokens.Jwt` v8.17.0
- `Microsoft.EntityFrameworkCore.Sqlite` v10.0.5
- `BCrypt.Net-Next` v4.1.0 (work factor 12)
- `FluentValidation` v12.1.1 + `FluentValidation.DependencyInjectionExtensions` v12.1.1
- `Asp.Versioning.Mvc` v8.1.1
- `Swashbuckle.AspNetCore` v7.3.1

**Storage**: SQLite; `Data Source=speckitdemo.db`; connection string in `appsettings.Development.json` (git-ignored)  
**Testing**: xUnit v2.9+, Moq v4.20+, FluentAssertions v7.x; no DB/HTTP in unit tests  
**Target Platform**: Local development / any OS supporting .NET 10  
**Project Type**: ASP.NET Core Web API — 7-project constitution layout  
**Performance Goals**: Login flow < 3 seconds end-to-end (SC-001)  
**Constraints**: Stateless tokens (no refresh tokens in v1); console-only JSON logging; no rate limiting in v1  
**Scale/Scope**: 2 public endpoints; 1 entity (`Member`); 3 seed users; single API version (v1)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify the following gates against the SpeckitDemo Constitution v2.0.0:

- [x] **Project Structure** — No project reference violations per the dependency
  matrix in Principle I. `SpeckitDemo.Api` may reference `SpeckitDemo.Data` and
  `SpeckitDemo.DataAccess` for host registration; endpoint classes do not use those types.
- [x] **SOLID** — New classes have a single responsibility; dependencies are
  injected via constructor; interfaces are narrow and role-specific.
- [x] **Repository Pattern** — All data-access is behind `I*Repository`
  interfaces in `SpeckitDemo.DataAccess/Abstractions/`; controllers do NOT call
  repositories directly.
- [x] **API Standards** — Endpoints follow RESTful conventions; responses use
  `ResultEnvelope<T>` envelope; request/response DTOs are used (no entity exposure).
- [x] **Exception Handling** — No try/catch for unhandled exceptions in
  controllers; global middleware handles all unhandled exceptions.
- [x] **Logging** — `ILogger<T>` used exclusively; no sensitive data in logs.
- [x] **EF Core Code-First** — Schema changes via migrations only; Fluent API
  configurations; no Data Annotations on domain entities.
- [x] **DI & Interfaces** — All services/repos injected via constructor; correct
  lifetimes (`Scoped` for repos/services, `Singleton` for stateless utils).
- [x] **Naming Conventions** — PascalCase classes/methods, `I` prefix on
  interfaces, `_camelCase` private fields, correct suffixes (Controller/Service/
  Repository/Request/Response).
- [x] **Testing** — Implementation written first; xUnit + Moq; AAA pattern;
  no DB/file/network access in unit tests; test method naming convention followed.
- [x] **Security** — FluentValidation on all request DTOs; JWT validated for
  sig/expiry/issuer/audience; no secrets committed; HTTPS enforced in non-dev.

*All gates pass. No violations requiring justification.*

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
<!--
  ACTION REQUIRED: Replace the placeholder tree below with the concrete layout
  for this feature. Delete unused options and expand the chosen structure with
  real paths (e.g., apps/admin, packages/something). The delivered plan must
  not include Option labels.
-->

```text
# Default: Project structure per constitution v2.0.0
src/
+-- SpeckitDemo.Contracts/     # Inbound/, Outbound/, Envelope/ResultEnvelope<T>
+-- SpeckitDemo.Data/          # SpeckitDemoContext, Mapping/, Members/, Migrations/
+-- SpeckitDemo.DataAccess/    # Abstractions/IAccountReader, Stores/AccountReader
+-- SpeckitDemo.Application/   # Abstractions/, Workflows/, Validation/
+-- SpeckitDemo.Security/      # Tokens/TokenIssuer, Passwords/SecretHasher
+-- SpeckitDemo.Api/           # Endpoints/v1/, Pipeline/, Hosting/, Program.cs

tests/
+-- SpeckitDemo.Tests/     # xUnit + Moq; no DB/HTTP access
```

**Structure Decision**: [Document the selected structure and reference the real
directories captured above]

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
