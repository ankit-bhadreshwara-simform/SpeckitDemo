# Implementation Plan: [FEATURE]

**Branch**: `[###-feature-name]` | **Date**: [DATE] | **Spec**: [link]
**Input**: Feature specification from `/specs/[###-feature-name]/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/plan-template.md` for the execution workflow.

## Summary

[Extract from feature spec: primary requirement + technical approach from research]

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 10 / C# 13+
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core 9.x, FluentValidation, Asp.Versioning.Mvc
**Storage**: SQLite (development/test); configurable via appsettings.json
**Testing**: xUnit (unit tests), Moq (mocking), WebApplicationFactory (integration tests, optional)
**Target Platform**: [e.g., Linux server / Windows / Azure App Service or NEEDS CLARIFICATION]
**Project Type**: ASP.NET Core Web API (SpeckitDemo.Api / SpeckitDemo.Contracts / SpeckitDemo.Data / SpeckitDemo.Security / SpeckitDemo.Application / SpeckitDemo.DataAccess / SpeckitDemo.Tests)
**Performance Goals**: [domain-specific, e.g., 1000 req/s, p95 <200ms or NEEDS CLARIFICATION]
**Constraints**: [domain-specific, e.g., <200ms p95, offline-capable or NEEDS CLARIFICATION]
**Scale/Scope**: [domain-specific, e.g., 10k users, 50 endpoints or NEEDS CLARIFICATION]

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Verify the following gates against the SpeckitDemo Constitution v2.0.0:

- [ ] **Project Structure** — No project reference violations per the dependency
  matrix in Principle I. `SpeckitDemo.Api` may reference `SpeckitDemo.Data` and
  `SpeckitDemo.DataAccess` for host registration; endpoint classes do not use those types.
- [ ] **SOLID** — New classes have a single responsibility; dependencies are
  injected via constructor; interfaces are narrow and role-specific.
- [ ] **Repository Pattern** — All data-access is behind `I*Repository`
  interfaces in `SpeckitDemo.DataAccess/Abstractions/`; controllers do NOT call
  repositories directly.
- [ ] **API Standards** — Endpoints follow RESTful conventions; responses use
  `ResultEnvelope<T>` envelope; request/response DTOs are used (no entity exposure).
- [ ] **Exception Handling** — No try/catch for unhandled exceptions in
  controllers; global middleware handles all unhandled exceptions.
- [ ] **Logging** — `ILogger<T>` used exclusively; no sensitive data in logs.
- [ ] **EF Core Code-First** — Schema changes via migrations only; Fluent API
  configurations; no Data Annotations on domain entities.
- [ ] **DI & Interfaces** — All services/repos injected via constructor; correct
  lifetimes (`Scoped` for repos/services, `Singleton` for stateless utils).
- [ ] **Naming Conventions** — PascalCase classes/methods, `I` prefix on
  interfaces, `_camelCase` private fields, correct suffixes (Controller/Service/
  Repository/Request/Response).
- [ ] **Testing** — Implementation written first; xUnit + Moq; AAA pattern;
  no DB/file/network access in unit tests; test method naming convention followed.
- [ ] **Security** — FluentValidation on all request DTOs; JWT validated for
  sig/expiry/issuer/audience; no secrets committed; HTTPS enforced in non-dev.

*Any gate violation that cannot be resolved MUST be documented in the
Complexity Tracking table below with explicit justification.*

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
