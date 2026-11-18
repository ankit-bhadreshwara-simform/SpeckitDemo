# Data Model: JWT Authentication API

**Feature**: `002-jwt-auth-api`  
**Date**: 2026-04-03  
**Status**: Complete

---

## Entities

### Member

The central domain entity. Persisted via EF Core Code-First in SQLite as the `Users` table. Stored in `SpeckitDemo.Data/Members/Member.cs`.

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| `Id` | `int` | PK, auto-increment, NOT NULL | Surrogate key |
| `Username` | `string` | UNIQUE, NOT NULL, max 100 chars | Used for login lookup |
| `PasswordHash` | `string` | NOT NULL, max 255 chars | BCrypt hash (work factor 12); never returned in responses |
| `FirstName` | `string` | NOT NULL, max 100 chars | Part of display name |
| `LastName` | `string` | NOT NULL, max 100 chars | Part of display name |
| `Email` | `string` | UNIQUE, NOT NULL, max 255 chars | Included in JWT claims and user details response |
| `Address` | `string` | NOT NULL, max 500 chars | User profile field |
| `CreatedDate` | `DateTime` | NOT NULL, default = UTC now | Set at entity creation; not updated |

**Entity class location**: `src/SpeckitDemo.Data/Members/Member.cs`

```csharp
namespace SpeckitDemo.Data.Members;

/// <summary>Represents a registered system user persisted in the database.</summary>
public class Member
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}
```

> **Note**: No Data Annotations are used. All schema constraints are defined via Fluent API in `MemberMap.cs` (Principle VI).

---

## Fluent API Configuration

**Location**: `src/SpeckitDemo.Data/Mapping/MemberMap.cs`

```csharp
namespace SpeckitDemo.Data.Mapping;

public sealed class MemberMap : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();

        builder.Property(u => u.Username)
            .IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Username).IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired().HasMaxLength(255);

        builder.Property(u => u.FirstName)
            .IsRequired().HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired().HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.Address)
            .IsRequired().HasMaxLength(500);

        builder.Property(u => u.CreatedDate)
            .IsRequired();

        // Seed data — pre-computed BCrypt hashes for password "admin" (work factor 12)
        // These hash strings are computed once and committed; they do NOT change on re-migration.
        builder.HasData(
            new Member
            {
                Id          = 1,
                Username    = "alice",
                FirstName   = "Alice",
                LastName    = "Smith",
                Email       = "alice@example.com",
                Address     = "123 Main Street, Springfield",
                PasswordHash = "<BCrypt.HashPassword(\"admin\", 12) — computed at migration time>",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Member
            {
                Id          = 2,
                Username    = "bob",
                FirstName   = "Bob",
                LastName    = "Jones",
                Email       = "bob@example.com",
                Address     = "456 Oak Avenue, Shelbyville",
                PasswordHash = "<BCrypt.HashPassword(\"admin\", 12) — computed at migration time>",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new Member
            {
                Id          = 3,
                Username    = "charlie",
                FirstName   = "Charlie",
                LastName    = "Brown",
                Email       = "charlie@example.com",
                Address     = "789 Pine Road, Capital City",
                PasswordHash = "<BCrypt.HashPassword(\"admin\", 12) — computed at migration time>",
                CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
```

> **Implementation note**: During `dotnet ef migrations add InitialCreate`, replace the `<BCrypt...>` placeholders with actual pre-computed hash strings (run `BCrypt.Net.BCrypt.HashPassword("admin", 12)` once in a scratch console and copy the output). This prevents spurious `UpdateData` migrations on subsequent `dotnet ef migrations add` calls.

---

## Bearer Token (Non-Persisted)

The Bearer Token is **not an entity** — it is a stateless, self-contained JWT issued at login and never stored in the database.

| Claim | JWT Claim Name | Value |
|-------|---------------|-------|
| User ID | `sub` | `Member.Id.ToString()` |
| Username | `name` | `Member.Username` |
| Email | `email` | `User.Email` |
| Token ID | `jti` | `Guid.NewGuid().ToString()` |
| Issued At | `iat` | UTC now (auto-set) |
| Expiry | `exp` | UTC now + `Jwt:ExpiryHours` (default: 1 hour) |

**Signing**: HS256 (HMAC-SHA256) with symmetric key from `Jwt:Key` configuration.

---

## Database Schema (inferred from EF Code-First)

```sql
CREATE TABLE "Users" (
    "Id"           INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Username"     TEXT    NOT NULL,
    "PasswordHash" TEXT    NOT NULL,
    "FirstName"    TEXT    NOT NULL,
    "LastName"     TEXT    NOT NULL,
    "Email"        TEXT    NOT NULL,
    "Address"      TEXT    NOT NULL,
    "CreatedDate"  TEXT    NOT NULL
);
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE UNIQUE INDEX "IX_Users_Email"    ON "Users" ("Email");
```

> EF Core generates this schema via `dotnet ef migrations add InitialCreate` → `dotnet ef database update`.

---

## State Transitions

The `Member` entity is **immutable** in v1 — no update or delete endpoints exist. The relevant state flow is:

```
[Seeded / Registered externally]
        │
        ▼
  User exists in DB
        │
  POST /api/v1/auth/login (with username + password)
        │
        ├── Valid credentials → JWT issued (stateless; not persisted)
        └── Invalid credentials → HTTP 401

  GET /api/v1/users/me (with Bearer token)
        │
        ├── Valid token + user still exists → HTTP 200 + AccountProfile
        ├── Valid token + user deleted      → HTTP 401 (re-validated per FR-013)
        ├── Expired / malformed token       → HTTP 401
        └── No token                        → HTTP 401
```

---

## Validation Rules

| Field | Rule | Source |
|-------|------|--------|
| `CredentialPayload.Username` | Required, not empty, max 100 chars | FR-002, FR-003 |
| `CredentialPayload.Password` | Required, not empty | FR-002 |
| Username lookup | Case-insensitive match against `Member.Username` | FR-003 (no user enumeration) |
| Password verification | `BCrypt.Verify(request.Password, user.PasswordHash)` | FR-012 |
| Token expiry | `ClockSkew = TimeSpan.Zero`; reject expired tokens | FR-011 |
| User existence on protected route | Query `IAccountReader.FindByIdAsync(userId)` on every request | FR-013 |
