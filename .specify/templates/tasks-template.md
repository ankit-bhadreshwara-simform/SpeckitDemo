---

description: "Task list template for feature implementation"
---

# Tasks: [FEATURE NAME]

**Input**: Design documents from `/specs/[###-feature-name]/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Default project structure (per constitution v2.0.0)**:
  `src/SpeckitDemo.Contracts/`, `src/SpeckitDemo.Data/`, `src/SpeckitDemo.DataAccess/`,
  `src/SpeckitDemo.Application/`, `src/SpeckitDemo.Security/`, `src/SpeckitDemo.Api/`,
  `tests/SpeckitDemo.Tests/`
- **Single flat project (spike only)**: `src/`, `tests/` at repository root
- Paths shown below assume the default layout — adjust based on plan.md structure

<!-- 
  ============================================================================
  IMPORTANT: The tasks below are SAMPLE TASKS for illustration purposes only.
  
  The /speckit.tasks command MUST replace these with actual tasks based on:
  - User stories from spec.md (with their priorities P1, P2, P3...)
  - Feature requirements from plan.md
  - Entities from data-model.md
  - Endpoints from contracts/
  
  Tasks MUST be organized by user story so each story can be:
  - Implemented independently
  - Tested independently
  - Delivered as an MVP increment
  
  DO NOT keep these sample tasks in the generated tasks.md file.
  ============================================================================
-->

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and Clean Architecture scaffolding

- [ ] T001 Create solution and seven-project structure per constitution
      (`SpeckitDemo.Contracts`, `SpeckitDemo.Data`, `SpeckitDemo.DataAccess`,
      `SpeckitDemo.Application`, `SpeckitDemo.Security`, `SpeckitDemo.Api`, `SpeckitDemo.Tests`)
- [ ] T002 Add project references following dependency matrix (see constitution
      Principle I; e.g., `SpeckitDemo.Api` -> `SpeckitDemo.Application` + `SpeckitDemo.Contracts`
      + `SpeckitDemo.Data` + `SpeckitDemo.DataAccess` + `SpeckitDemo.Security`;
      `SpeckitDemo.DataAccess` -> `SpeckitDemo.Data`;
      `SpeckitDemo.Application` -> `SpeckitDemo.Contracts` + `SpeckitDemo.DataAccess` + `SpeckitDemo.Security`)
- [ ] T003 [P] Configure `<Nullable>enable</Nullable>` and
      `<ImplicitUsings>enable</ImplicitUsings>` in all `.csproj` files
- [ ] T004 [P] Add NuGet packages: EF Core + SQLite, FluentValidation,
      Asp.Versioning.Mvc, JwtBearer, xUnit, Moq

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**Constitution gates that must pass before user story work begins:**
- Project reference matrix validated (no violations per Principle I)
- ResultEnvelope<T> common envelope implemented in SpeckitDemo.Contracts/Envelope/
- UnhandledErrorMiddleware registered in SpeckitDemo.Api
- EF Core + SQLite DbContext + initial migration in place in SpeckitDemo.Data
- JWT authentication middleware configured via SpeckitDemo.Security
- FluentValidation pipeline behavior registered in DI

Examples of foundational tasks (adjust based on your feature):

- [ ] T005 Implement `ResultEnvelope<T>` and `ResultEnvelope` (non-generic) in
      `src/SpeckitDemo.Contracts/Envelope/ResultEnvelope.cs`
- [ ] T006 [P] Create `SpeckitDemoContext` in
      `src/SpeckitDemo.Data/SpeckitDemoContext.cs`
- [ ] T007 [P] Implement `UnhandledErrorMiddleware` in
      `src/SpeckitDemo.Api/Pipeline/UnhandledErrorMiddleware.cs`
- [ ] T008 Configure JWT Bearer authentication in
      `src/SpeckitDemo.Security/Tokens/TokenIssuer.cs` and register in
      `src/SpeckitDemo.Api/Program.cs`
- [ ] T009 Register FluentValidation pipeline behavior in DI
- [ ] T010 Configure API versioning (`Asp.Versioning.Mvc`) with default `v1`
- [ ] T011 Add `appsettings.Development.json` to `.gitignore` (secrets file)

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - [Title] (Priority: P1) 🎯 MVP

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 1 (OPTIONAL - only if tests requested) ⚠️

> **NOTE: Write implementation FIRST. Write these tests IMMEDIATELY AFTER
> each class is implemented, before moving to the next class.**
> Use xUnit + Moq. AAA pattern. No DB/file/network access.
> Naming: `MethodName_StateUnderTest_ExpectedBehaviour`

- [ ] T012 [P] [US1] Unit tests for [ServiceClass] in
      `tests/SpeckitDemo.Tests/Workflows/[ServiceClass]Tests.cs`
- [ ] T013 [P] [US1] Unit tests for [ValidatorClass] in
      `tests/SpeckitDemo.Tests/Validation/[ValidatorClass]Tests.cs`

### Implementation for User Story 1

- [ ] T014 [P] [US1] Create `[Entity1]` entity in
      `src/SpeckitDemo.Data/Members/[Entity1].cs`
- [ ] T015 [P] [US1] Create `I[Entity1]Repository` interface in
      `src/SpeckitDemo.DataAccess/Abstractions/I[Entity1]Repository.cs`
- [ ] T016 [P] [US1] Create request/response DTOs in
      `src/SpeckitDemo.Contracts/Inbound/` and `src/SpeckitDemo.Contracts/Outbound/`
- [ ] T017 [US1] Create FluentValidation validator for request DTO in
      `src/SpeckitDemo.Application/Validation/[RequestName]Validator.cs`
- [ ] T018 [US1] Implement `I[Feature]Service` interface and `[Feature]Service`
      in `src/SpeckitDemo.Application/` (depends on T015, T016)
- [ ] T019 [US1] Implement `[Entity1]Repository` in
      `src/SpeckitDemo.DataAccess/Stores/`
- [ ] T020 [US1] Add `IEntityTypeConfiguration<[Entity1]>` in
      `src/SpeckitDemo.Data/Mapping/`
- [ ] T021 [US1] Add EF Core migration for [Entity1] schema
- [ ] T022 [US1] Implement `[Feature]Controller` in
      `src/SpeckitDemo.Api/Endpoints/v1/[Feature]Controller.cs`
- [ ] T023 [US1] Register new services/repos in DI (Program.cs)
- [ ] T024 [US1] Unit tests for `[Feature]Service` — write after T018 complete

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - [Title] (Priority: P2)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 2 (OPTIONAL - only if tests requested) ⚠️

> **NOTE: Write implementation FIRST. Unit tests immediately after each class.**

- [ ] T025 [P] [US2] Unit tests for [ServiceClass] in
      `tests/SpeckitDemo.Tests/Workflows/[ServiceClass]Tests.cs`

### Implementation for User Story 2

- [ ] T026 [P] [US2] Create domain entity / DTOs for User Story 2
- [ ] T027 [P] [US2] Create `I[Feature2]Service` interface and FluentValidation validator
- [ ] T028 [US2] Implement `[Feature2]Service` in Application layer
- [ ] T029 [US2] Implement repository and EF Core configuration for new entities
- [ ] T030 [US2] Add EF Core migration for User Story 2 schema changes
- [ ] T031 [US2] Implement `[Feature2]Controller` in `src/SpeckitDemo.Api/Endpoints/v1/`
- [ ] T032 [US2] Unit tests for `[Feature2]Service` — write after T028 complete
- [ ] T033 [US2] Integrate with User Story 1 components (if needed)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - [Title] (Priority: P3)

**Goal**: [Brief description of what this story delivers]

**Independent Test**: [How to verify this story works on its own]

### Tests for User Story 3 (OPTIONAL - only if tests requested) ⚠️

> **NOTE: Write implementation FIRST. Unit tests immediately after each class.**

- [ ] T034 [P] [US3] Unit tests for [ServiceClass] in
      `tests/SpeckitDemo.Tests/Workflows/[ServiceClass]Tests.cs`

### Implementation for User Story 3

- [ ] T035 [P] [US3] Create domain entity / DTOs for User Story 3
- [ ] T036 [P] [US3] Create `I[Feature3]Service` interface and FluentValidation validator
- [ ] T037 [US3] Implement `[Feature3]Service` in Application layer
- [ ] T038 [US3] Implement repository and EF Core configuration for new entities
- [ ] T039 [US3] Add EF Core migration for User Story 3 schema changes
- [ ] T040 [US3] Implement `[Feature3]Controller` in `src/SpeckitDemo.Api/Endpoints/v1/`
- [ ] T041 [US3] Unit tests for `[Feature3]Service` — write after T037 complete

**Checkpoint**: All user stories should now be independently functional

---

[Add more user story phases as needed, following the same pattern]

---

## Phase N: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] TXXX [P] Documentation updates in docs/
- [ ] TXXX Code cleanup and refactoring
- [ ] TXXX Performance optimization across all stories
- [ ] TXXX [P] Additional unit tests (if requested) in tests/unit/
- [ ] TXXX Security hardening
- [ ] TXXX Run quickstart.md validation

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1 but should be independently testable
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together (if tests requested):
Task: "Contract test for [endpoint] in tests/contract/test_[name].py"
Task: "Integration test for [user journey] in tests/integration/test_[name].py"

# Launch all models for User Story 1 together:
Task: "Create [Entity1] model in src/models/[entity1].py"
Task: "Create [Entity2] model in src/models/[entity2].py"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1
   - Developer B: User Story 2
   - Developer C: User Story 3
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
