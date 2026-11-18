# Feature Specification: JWT Authentication API

**Feature Branch**: `002-jwt-auth-api`  
**Created**: 2026-04-03  
**Status**: Draft  
**Input**: User description: "Create a new project and implement JWT authentication. The project should include: A login API where the user enters username and password; On successful login, return a Bearer token; A GET API for authenticated users to fetch their details; Proper project structure as per the constitution; Use SQLite database and Entity Framework Core; Follow coding standards, dependency injection, logging, and global error handling; Include basic unit tests using xUnit and Moq"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - User Login and Token Issuance (Priority: P1)

A registered user submits their username and password to the login endpoint. If the credentials are valid, the system returns a Bearer token that the user can include in subsequent requests. If credentials are invalid, the system returns a clear, secure error message without revealing which field was wrong.

**Why this priority**: Authentication is the foundation of the entire feature. No other story is possible without a working token issuance mechanism. It delivers immediate, demonstrable value as a standalone login flow.

**Independent Test**: Can be fully tested by sending a POST request to the login endpoint with valid credentials and confirming a Bearer token is returned, and by sending invalid credentials and confirming an appropriate error response is returned.

**Acceptance Scenarios**:

1. **Given** a registered user with valid credentials, **When** they POST their username and password to the login endpoint, **Then** the system returns a Bearer token along with an HTTP 200 response.
2. **Given** a user with an incorrect password, **When** they POST to the login endpoint, **Then** the system returns HTTP 401 with a generic "invalid credentials" message and does not reveal which field was wrong.
3. **Given** a user submits an empty username or password, **When** the request reaches the system, **Then** the system returns HTTP 400 with validation error details before any credential check occurs.
4. **Given** a username that does not exist, **When** the user POSTs to the login endpoint, **Then** the system returns HTTP 401 with the same generic error message as an incorrect password (no user enumeration).

---

### User Story 2 - Fetch Authenticated User Details (Priority: P2)

An authenticated user includes their Bearer token in the Authorization header and calls the user details endpoint. The system validates the token, identifies the user, and returns their profile information. If the token is missing, expired, or tampered with, access is denied.

**Why this priority**: This is the primary protected resource the feature exposes. It validates the full end-to-end authentication flow and demonstrates that token-based access control works correctly.

**Independent Test**: Can be fully tested by first obtaining a token via the login endpoint (US1), then sending a GET request with that token to the user details endpoint and confirming the correct user data is returned.

**Acceptance Scenarios**:

1. **Given** an authenticated user with a valid Bearer token, **When** they send a GET request to the user details endpoint, **Then** the system returns their profile information with HTTP 200.
2. **Given** a request without an Authorization header, **When** the GET request reaches the user details endpoint, **Then** the system returns HTTP 401 and denies access.
3. **Given** a request with an expired token, **When** the GET request reaches the user details endpoint, **Then** the system returns HTTP 401 with an appropriate expiry message.
4. **Given** a request with a malformed or tampered token, **When** the GET request reaches the user details endpoint, **Then** the system returns HTTP 401.
5. **Given** a request with a valid, unexpired token for a user who has since been deleted, **When** the GET request reaches the user details endpoint, **Then** the system returns HTTP 401.

---

### Edge Cases

- What happens when the login endpoint receives a payload with extra unexpected fields?
- How does the system handle multiple rapid login attempts with the same credentials?
- What happens if the SQLite database is unavailable at startup?
- How does the system behave when the token signing key is missing from configuration?
- What happens if a valid token is presented for a user who no longer exists in the database? → The system re-validates user existence on every protected request and returns HTTP 401 if the user is not found.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a public login endpoint that accepts a username and password and, on success, returns a Bearer token signed with HMAC-SHA256 (HS256).
- **FR-002**: System MUST validate all login request inputs (username and password are required, non-empty) before performing any credential lookup.
- **FR-003**: System MUST return a generic "invalid credentials" error for both wrong password and unknown username, preventing user enumeration.
- **FR-004**: System MUST expose a protected user details endpoint that requires a valid Bearer token; unauthenticated requests MUST be rejected with HTTP 401.
- **FR-005**: System MUST return the authenticated user's profile information (at minimum: user identifier, username, display name, and email address) from the user details endpoint.
- **FR-006**: System MUST return all API responses wrapped in a standard envelope structure (success flag, data payload, and error details when applicable).
- **FR-007**: System MUST persist user records using a relational data store with a Code-First schema migration approach.
- **FR-008**: System MUST apply structured logging (JSON-formatted output to stdout/console) at meaningful points: request handling, authentication success/failure, and unhandled exceptions.
- **FR-009**: System MUST handle all unhandled exceptions through a centralized global error handler that returns a safe, structured error response without exposing stack traces.
- **FR-010**: System MUST include unit tests covering the login service logic and token validation logic using test doubles for all external dependencies.
- **FR-011**: Tokens MUST carry an expiry; the system MUST reject expired tokens at the protected endpoint.
- **FR-012**: Passwords MUST NOT be stored in plain text; they MUST be hashed using BCrypt (via `BCrypt.Net-Next`) with a configurable work factor before persistence.
- **FR-013**: On every request to the protected user details endpoint, the system MUST verify that the user identified by the token still exists in the database; if the user no longer exists, the system MUST return HTTP 401.

### Key Entities

- **User**: Represents a registered system user. Key attributes: unique identifier, username, hashed password, display name, and email address. A user has exactly one set of credentials.
- **Bearer Token**: A self-contained, HS256-signed, time-limited JWT issued upon successful login. Carries the user's identity claims. Not persisted — stateless.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A user can complete the full login flow (submit credentials, receive token) in under 3 seconds under normal conditions.
- **SC-002**: The protected user details endpoint correctly returns user data for 100% of requests carrying a valid, unexpired token.
- **SC-003**: The protected user details endpoint rejects 100% of requests with missing, expired, or malformed tokens with an appropriate error response.
- **SC-004**: Invalid login attempts (wrong password or unknown username) always return the same generic error message, with no difference in response body between the two cases.
- **SC-005**: All API error responses follow a consistent structure — no raw exception messages or stack traces are ever exposed to callers.
- **SC-006**: Unit test suite achieves coverage of all primary success and failure paths in the authentication service logic.
- **SC-007**: Application starts up cleanly from a fresh checkout with a single migration command and no manual configuration beyond providing a token signing secret.

## Assumptions

- Users are pre-seeded or registered by a separate out-of-scope process; this feature does not include a user registration endpoint.
- A single user role is sufficient for v1 — no role-based or permission-based access control is in scope.
- Token expiry duration follows a reasonable industry default (e.g., 60 minutes); the exact value is configurable via application settings.
- The SQLite database file is stored locally on the server; no remote database connectivity is required.
- The token signing secret is a symmetric key (used for HS256) provided through application configuration (e.g., environment variable or settings file) and is never hard-coded.
- Refresh token support (issuing new tokens without re-login) is out of scope for v1.
- Rate limiting and account lockout after repeated failed logins are out of scope for v1 but should be considered for production hardening.
- The API is intended for consumption by client applications (web/mobile); no browser-based session cookie authentication is required.

## Clarifications

### Session 2026-04-03

- Q: Which user profile attributes should be stored and returned from the user details endpoint — display name, email, or both? → A: Both email address and display name are stored on the User entity and returned by the user details endpoint.
- Q: Which password hashing algorithm should be used to satisfy FR-012? → A: BCrypt via `BCrypt.Net-Next` with a configurable work factor.
- Q: Which JWT signing algorithm should be used for token issuance and verification? → A: HS256 (HMAC-SHA256) with a symmetric shared secret from application configuration.
- Q: What should the system do when a valid token is presented for a user who no longer exists in the database? → A: Reject with HTTP 401; user existence is re-validated on every protected request.
- Q: What structured logging sink should be used for FR-008? → A: Console only — JSON-formatted structured output to stdout; no file or external sink required.
