# Research: JWT Authentication API

**Feature**: `002-jwt-auth-api`  
**Date**: 2026-04-03  
**Status**: Complete — all NEEDS CLARIFICATION resolved

---

## 1. BCrypt Password Hashing

**Decision**: Use `BCrypt.Net-Next` v4.1.0 with work factor **12**.

**Rationale**: Work factor 12 produces ~100–300 ms hash time on modern hardware, meeting the OWASP 2025 recommendation. The library default (11) is now below the security baseline. v4.1.0 includes a `net10.0` target frame.

**Key API**:
```csharp
// Hash a password
string hash = BCrypt.Net.BCrypt.HashPassword(plainText, workFactor: 12);

// Verify a password
bool isValid = BCrypt.Net.BCrypt.Verify(plainText, hash);
```

**NuGet package**: `BCrypt.Net-Next` v4.1.0

**Alternatives considered**: `Microsoft.AspNetCore.Identity` password hasher — rejected because it introduces Identity framework overhead not needed here; `Argon2` — out of scope per FR-012 which explicitly mandates BCrypt via `BCrypt.Net-Next`.

---

## 2. EF Core 10 + SQLite Code-First

**Decision**: Use `Microsoft.EntityFrameworkCore.Sqlite` v10.0.5 and `Microsoft.EntityFrameworkCore.Design` v10.0.5.

**Rationale**: EF Core versions are tied to the .NET runtime version. v10.0.5 is the current stable release for .NET 10.

**DbContext registration in `Program.cs`**:
```csharp
builder.Services.AddDbContext<SpeckitDemoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

**`appsettings.json` connection string**:
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=speckitdemo.db"
}
```

**NuGet packages**:
- `Microsoft.EntityFrameworkCore.Sqlite` v10.0.5
- `Microsoft.EntityFrameworkCore.Design` v10.0.5 (PrivateAssets="all", to `SpeckitDemo.Data` only)
- Global tool: `dotnet-ef` v10.0.5

**Alternatives considered**: PostgreSQL — rejected (spec mandates SQLite); in-memory provider — rejected (not appropriate for production).

---

## 3. EF Core Seeding Strategy

**Decision**: Use `HasData()` in `IEntityTypeConfiguration<User>` with **pre-computed static BCrypt hash literals** baked at plan time. Passwords for all seed users will be `admin` hashed once (outside migrations) and stored as a string constant.

**Rationale**: The spec requires 2–3 seed users with password `admin`. `HasData()` is appropriate for static reference data. The critical caveat — that calling `BCrypt.HashPassword()` inside `HasData()` generates a new random salt on every `dotnet ef migrations add` and produces spurious `UpdateData` migrations — is avoided by computing hashes **once** and hardcoding the resulting hash string literal.

**Pre-computed hash for `admin` (work factor 12)**:
```
$2a$12$...  (computed once at setup time by running BCrypt.HashPassword("admin", 12))
```
> The actual hash string will be computed in the migration seed helper and committed as a constant in `SeedData.cs` inside `SpeckitDemo.Data/Mapping/`.

**Seed entity structure** (User IDs fixed for `HasData`):
| Id | Username | FirstName | LastName | PasswordHash | Email | Address | CreatedDate |
|----|----------|-----------|----------|--------------|-------|---------|-------------|
| 1 | alice | Alice | Smith | `<BCrypt hash of "admin">` | alice@example.com | 123 Main St | 2026-01-01 |
| 2 | bob | Bob | Jones | `<BCrypt hash of "admin">` | bob@example.com | 456 Oak Ave | 2026-01-01 |
| 3 | charlie | Charlie | Brown | `<BCrypt hash of "admin">` | charlie@example.com | 789 Pine Rd | 2026-01-01 |

**Alternative considered**: `UseSeeding()` callback (EF Core 9+ recommended path) — deferred because `HasData()` keeps seed data version-controlled in migrations, which aligns with Principle VI ("Migrations MUST be committed to source control").

---

## 4. JWT Bearer Authentication

**Decision**: Use `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.5 (separate NuGet, not bundled in SDK) and `System.IdentityModel.Tokens.Jwt` v8.17.0 for token generation.

**Rationale**: The spec mandates HS256 with a symmetric signing key from configuration (FR-001, FR-011). JwtBearerDefaults scheme is the standard approach for Web API projects.

**`Program.cs` configuration**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew                = TimeSpan.Zero   // eliminate 5-min leeway for expiry
        };
    });
```

**Token generation** (in `SpeckitDemo.Security/Tokens/TokenIssuer.cs`):
```csharp
var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
    new Claim(JwtRegisteredClaimNames.Name,  user.Username),
    new Claim(JwtRegisteredClaimNames.Email, user.Email),
    new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
};
var token = new JwtSecurityToken(
    issuer:             config["Jwt:Issuer"],
    audience:           config["Jwt:Audience"],
    claims:             claims,
    expires:            DateTime.UtcNow.AddHours(expiryHours),
    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
return new JwtSecurityTokenHandler().WriteToken(token);
```

**`appsettings.json` JWT section**:
```json
"Jwt": {
  "Issuer": "SpeckitDemo",
  "Audience": "SpeckitDemo",
  "Key": "<secret — stored in appsettings.Development.json only>",
  "ExpiryHours": 1
}
```

**NuGet packages**:
- `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.5
- `System.IdentityModel.Tokens.Jwt` v8.17.0

**Alternatives considered**: RS256 asymmetric signing — deferred per spec assumption (symmetric key is sufficient for v1); `JsonWebTokenHandler` (IdentityModel 8.x async API) — deferred, `JwtSecurityTokenHandler` works for sync scenarios and is more familiar.

---

## 5. FluentValidation

**Decision**: Use `FluentValidation` v12.1.1 + `FluentValidation.DependencyInjectionExtensions` v12.1.1. Use **manual validation** inside service layer (not auto-validation).

**Rationale**: `FluentValidation.AspNetCore` (which provided `AddFluentValidationAutoValidation()`) is officially deprecated as of v11.3.1. Per constitution Principle X, all request DTOs must be validated via FluentValidation before reaching service logic. The recommended pattern for .NET 10 is to inject `IValidator<T>` into the service and call `ValidateAsync()` explicitly.

**Registration**:
```csharp
builder.Services.AddValidatorsFromAssemblyContaining<CredentialRules>();
```

**Validator pattern**:
```csharp
public sealed class CredentialRules : AbstractValidator<CredentialPayload>
{
    public CredentialRules()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty();
    }
}
```

**Manual invocation in service**:
```csharp
var result = await _validator.ValidateAsync(request);
if (!result.IsValid)
    return ResultEnvelope<IssuedToken>.Fail("Validation failed",
        result.Errors.Select(e => e.ErrorMessage));
```

**NuGet packages**:
- `FluentValidation` v12.1.1
- `FluentValidation.DependencyInjectionExtensions` v12.1.1

**Alternatives considered**: `FluentValidation.AspNetCore` auto-validation — deprecated and unsupported; data annotations — prohibited by constitution Principle VI ("Data Annotations on domain entities are prohibited").

---

## 6. API Versioning

**Decision**: Use `Asp.Versioning.Mvc` v8.1.1 with URL path versioning (`/api/v1/`).

**Rationale**: Constitution Principle IV mandates URL path versioning. `Asp.Versioning.Mvc` v8.1.1 targets .NET 8+ and is fully compatible with .NET 10.

**Registration**:
```csharp
builder.Services.AddApiVersioning().AddMvc();
```

**Controller decoration**:
```csharp
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class SignInController : ControllerBase { ... }
```

**NuGet package**: `Asp.Versioning.Mvc` v8.1.1

---

## 7. Global Exception Handling

**Decision**: Implement a custom `UnhandledErrorMiddleware` class registered in `Program.cs` via `app.UseMiddleware<UnhandledErrorMiddleware>()`.

**Rationale**: Constitution Principle V requires a single global exception-handling middleware. Controllers MUST NOT contain try/catch for unhandled exceptions. The middleware catches all unhandled exceptions, logs them with `ILogger<T>` at `LogError` level, and returns a structured `ResultEnvelope<object>` error envelope with HTTP 500.

**Pattern**:
```csharp
public class UnhandledErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UnhandledErrorMiddleware> _logger;

    public UnhandledErrorMiddleware(RequestDelegate next,
        ILogger<UnhandledErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var response = ResultEnvelope<object>.Fail("An unexpected error occurred.");
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
```

---

## 8. Structured Logging

**Decision**: Use the built-in `Microsoft.Extensions.Logging` with the **JSON console formatter** (`AddJsonConsole()`).

**Rationale**: Spec FR-008 mandates JSON-formatted structured output to stdout. The built-in JSON formatter satisfies this with zero additional NuGet packages.

**Configuration in `Program.cs`**:
```csharp
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ssZ";
});
```

**Alternatives considered**: Serilog, NLog — out of scope; spec mandates console only; built-in JSON formatter is sufficient.

---

## 9. Unit Testing Approach

**Decision**: xUnit v2.9+ with Moq v4.20+ following AAA pattern. Tests for `SignInWorkflow` cover all primary success/failure paths via mocked `IAccountReader` and `ITokenIssuer`.

**Test method naming convention**: `MethodName_StateUnderTest_ExpectedBehaviour`

**Key test scenarios (SignInWorkflow)**:
1. `Login_ValidCredentials_ReturnsTokenResponse`
2. `Login_InvalidPassword_ReturnsFailResponse`
3. `Login_UsernameNotFound_ReturnsFailResponse`
4. `Login_EmptyUsername_ReturnsValidationFailure`
5. `GetUserDetails_ValidTokenUser_ReturnsUserProfile`
6. `GetUserDetails_UserDeletedAfterToken_ReturnsFailResponse`

**NuGet packages** (in `SpeckitDemo.Tests`):
- `xunit` v2.9+
- `xunit.runner.visualstudio` v2.9+
- `Moq` v4.20+
- `FluentAssertions` v7.x (optional but recommended for readable assertions)
- `Microsoft.NET.Test.Sdk` latest

---

## Resolution Summary

| Unknown | Resolution |
|---------|-----------|
| BCrypt work factor | **12** (industry baseline 2025) |
| EF Core version for .NET 10 | **10.0.5** |
| Seeding with hashed passwords | `HasData()` with **pre-computed static hash literals** |
| JWT package | `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.5 (separate NuGet) |
| FluentValidation approach | Manual `ValidateAsync()` in service; `FluentValidation.AspNetCore` deprecated |
| API versioning package | `Asp.Versioning.Mvc` v8.1.1 |
| Structured logging sink | Built-in `AddJsonConsole()` — no extra packages |
| Global exception handling | Custom `UnhandledErrorMiddleware` in `SpeckitDemo.Api/Pipeline/` |
