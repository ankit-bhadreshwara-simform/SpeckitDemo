# API Contracts: JWT Authentication API

**Feature**: `002-jwt-auth-api`  
**Base URL**: `/api/v1`  
**Date**: 2026-04-03

All responses are wrapped in the standard `ResultEnvelope<T>` envelope defined in `SpeckitDemo.Contracts/Envelope/ResultEnvelope.cs`.

---

## Standard Response Envelope

```json
{
  "success": true | false,
  "data": <T> | null,
  "message": "Human-readable message",
  "errors": ["error detail 1", "error detail 2"]
}
```

---

## Endpoint 1: Login

### `POST /api/v1/auth/login`

Accepts username and password, validates credentials, and returns a signed Bearer token.

**Authentication required**: No (public endpoint)

---

#### Request

**Content-Type**: `application/json`

**DTO**: `CredentialPayload` (`SpeckitDemo.Contracts/Inbound/CredentialPayload.cs`)

```json
{
  "username": "alice",
  "password": "admin"
}
```

| Field | Type | Required | Validation |
|-------|------|----------|------------|
| `username` | `string` | Yes | Not empty; max 100 characters |
| `password` | `string` | Yes | Not empty |

---

#### Responses

**`200 OK` — Successful login**

**DTO**: `ResultEnvelope<IssuedToken>`  
**DTO source**: `SpeckitDemo.Contracts/Outbound/IssuedToken.cs`

```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "tokenType": "Bearer",
    "expiresAt": "2026-04-03T13:00:00Z"
  },
  "message": "Login successful",
  "errors": []
}
```

| Field | Type | Notes |
|-------|------|-------|
| `token` | `string` | Signed HS256 JWT; include as `Authorization: Bearer <token>` |
| `tokenType` | `string` | Always `"Bearer"` |
| `expiresAt` | `string` (ISO 8601 UTC) | Token expiry timestamp |

---

**`400 Bad Request` — Validation failure**

Returned when `username` or `password` is missing or empty (before credential check).

```json
{
  "success": false,
  "data": null,
  "message": "Validation failed",
  "errors": [
    "Username is required.",
    "Password is required."
  ]
}
```

---

**`401 Unauthorized` — Invalid credentials**

Returned for both wrong password AND unknown username (no user enumeration — identical response for both cases, per FR-003).

```json
{
  "success": false,
  "data": null,
  "message": "Invalid credentials",
  "errors": []
}
```

---

**`500 Internal Server Error` — Unhandled exception**

```json
{
  "success": false,
  "data": null,
  "message": "An unexpected error occurred.",
  "errors": []
}
```

---

## Endpoint 2: Get Authenticated User Details

### `GET /api/v1/users/me`

Returns the profile information of the currently authenticated user. Re-validates user existence on every call (FR-013).

**Authentication required**: Yes — `Authorization: Bearer <token>`

---

#### Request

**Headers**:

| Header | Required | Value |
|--------|----------|-------|
| `Authorization` | Yes | `Bearer <JWT token>` |

No request body.

---

#### Responses

**`200 OK` — User details returned**

**DTO**: `ResultEnvelope<AccountProfile>`  
**DTO source**: `SpeckitDemo.Contracts/Outbound/AccountProfile.cs`

```json
{
  "success": true,
  "data": {
    "id": 1,
    "username": "alice",
    "firstName": "Alice",
    "lastName": "Smith",
    "email": "alice@example.com",
    "address": "123 Main Street, Springfield",
    "createdDate": "2026-01-01T00:00:00Z"
  },
  "message": "User details retrieved successfully",
  "errors": []
}
```

| Field | Type | Notes |
|-------|------|-------|
| `id` | `int` | User's database identifier |
| `username` | `string` | Login username |
| `firstName` | `string` | User first name |
| `lastName` | `string` | User last name |
| `email` | `string` | User email address |
| `address` | `string` | User address |
| `createdDate` | `string` (ISO 8601 UTC) | Account creation date |

> **Security note**: `passwordHash` is **never** included in any API response.

---

**`401 Unauthorized` — No token provided**

```json
{
  "success": false,
  "data": null,
  "message": "Authentication required",
  "errors": []
}
```

---

**`401 Unauthorized` — Token expired**

```json
{
  "success": false,
  "data": null,
  "message": "Token has expired",
  "errors": []
}
```

---

**`401 Unauthorized` — Token malformed or tampered**

```json
{
  "success": false,
  "data": null,
  "message": "Invalid token",
  "errors": []
}
```

---

**`401 Unauthorized` — User no longer exists (FR-013)**

Returned when the token is valid and unexpired but the user has been deleted from the database.

```json
{
  "success": false,
  "data": null,
  "message": "User not found",
  "errors": []
}
```

---

**`500 Internal Server Error` — Unhandled exception**

```json
{
  "success": false,
  "data": null,
  "message": "An unexpected error occurred.",
  "errors": []
}
```

---

## DTO Definitions

### `CredentialPayload`

**File**: `src/SpeckitDemo.Contracts/Inbound/CredentialPayload.cs`

```csharp
namespace SpeckitDemo.Contracts.Inbound;

/// <summary>Request body for the login endpoint.</summary>
public sealed class CredentialPayload
{
    /// <summary>The user's login username.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The user's plain-text password (transmitted over HTTPS only).</summary>
    public string Password { get; set; } = string.Empty;
}
```

---

### `IssuedToken`

**File**: `src/SpeckitDemo.Contracts/Outbound/IssuedToken.cs`

```csharp
namespace SpeckitDemo.Contracts.Outbound;

/// <summary>Response body returned on successful authentication.</summary>
public sealed class IssuedToken
{
    /// <summary>The signed JWT Bearer token.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Token type — always "Bearer".</summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>UTC timestamp when the token expires.</summary>
    public DateTime ExpiresAt { get; set; }
}
```

---

### `AccountProfile`

**File**: `src/SpeckitDemo.Contracts/Outbound/AccountProfile.cs`

```csharp
namespace SpeckitDemo.Contracts.Outbound;

/// <summary>Response body for the authenticated user details endpoint.</summary>
public sealed class AccountProfile
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
```

---

### `ResultEnvelope<T>` (Standard Envelope)

**File**: `src/SpeckitDemo.Contracts/Envelope/ResultEnvelope.cs`

```csharp
namespace SpeckitDemo.Contracts.Envelope;

/// <summary>Standard API response envelope used by all endpoints.</summary>
public sealed class ResultEnvelope<T>
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

---

## Error Code Reference

| HTTP Status | Scenario | `message` value |
|-------------|----------|----------------|
| 400 | Validation failure (empty username/password) | `"Validation failed"` |
| 401 | Invalid credentials (wrong password or unknown user) | `"Invalid credentials"` |
| 401 | No Authorization header | `"Authentication required"` |
| 401 | Token expired | `"Token has expired"` |
| 401 | Token malformed/tampered | `"Invalid token"` |
| 401 | User deleted after token issued | `"User not found"` |
| 500 | Unhandled exception | `"An unexpected error occurred."` |
