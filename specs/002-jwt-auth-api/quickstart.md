# Quickstart: JWT Authentication API

**Feature**: `002-jwt-auth-api`  
**Date**: 2026-04-03

Get from fresh checkout to a running, tested API in under 5 minutes.

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 10.x | `dotnet --version` to verify |
| dotnet-ef (global tool) | 10.0.5 | `dotnet tool install --global dotnet-ef --version 10.0.5` |
| Git | Any | Repo already cloned |

---

## 1. Clone and Open

```powershell
git clone <repo-url>
cd SpeckitDemo
git checkout 002-jwt-auth-api
```

---

## 2. Create the Solution and Projects

Run from the repository root. These commands scaffold the full project structure per constitution v2.0.0.

```powershell
# Create solution
dotnet new sln -n SpeckitDemo

# Source projects
dotnet new classlib -n SpeckitDemo.Contracts      -o src/SpeckitDemo.Contracts      -f net10.0
dotnet new classlib -n SpeckitDemo.Data  -o src/SpeckitDemo.Data  -f net10.0
dotnet new classlib -n SpeckitDemo.DataAccess -o src/SpeckitDemo.DataAccess -f net10.0
dotnet new classlib -n SpeckitDemo.Application   -o src/SpeckitDemo.Application   -f net10.0
dotnet new classlib -n SpeckitDemo.Security   -o src/SpeckitDemo.Security   -f net10.0
dotnet new webapi   -n SpeckitDemo.Api       -o src/SpeckitDemo.Api       -f net10.0

# Test project
dotnet new xunit    -n SpeckitDemo.Tests  -o tests/SpeckitDemo.Tests -f net10.0

# Add all to solution
dotnet sln add src/SpeckitDemo.Contracts/SpeckitDemo.Contracts.csproj
dotnet sln add src/SpeckitDemo.Data/SpeckitDemo.Data.csproj
dotnet sln add src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj
dotnet sln add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj
dotnet sln add src/SpeckitDemo.Security/SpeckitDemo.Security.csproj
dotnet sln add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj
dotnet sln add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj
```

---

## 3. Add Project References (Dependency Matrix)

```powershell
# SpeckitDemo.DataAccess references SpeckitDemo.Data
dotnet add src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj reference src/SpeckitDemo.Data/SpeckitDemo.Data.csproj

# SpeckitDemo.Application references SpeckitDemo.Contracts + SpeckitDemo.DataAccess + SpeckitDemo.Security
dotnet add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj reference src/SpeckitDemo.Contracts/SpeckitDemo.Contracts.csproj
dotnet add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj reference src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj
dotnet add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj reference src/SpeckitDemo.Security/SpeckitDemo.Security.csproj

# SpeckitDemo.Api references Application, Contracts, Data, DataAccess, and Security
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj reference src/SpeckitDemo.Application/SpeckitDemo.Application.csproj
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj reference src/SpeckitDemo.Contracts/SpeckitDemo.Contracts.csproj
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj reference src/SpeckitDemo.Data/SpeckitDemo.Data.csproj
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj reference src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj reference src/SpeckitDemo.Security/SpeckitDemo.Security.csproj

# SpeckitDemo.Tests references the same libraries, not the API host
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj reference src/SpeckitDemo.Application/SpeckitDemo.Application.csproj
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj reference src/SpeckitDemo.Contracts/SpeckitDemo.Contracts.csproj
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj reference src/SpeckitDemo.Data/SpeckitDemo.Data.csproj
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj reference src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj reference src/SpeckitDemo.Security/SpeckitDemo.Security.csproj
```

---

## 4. Install NuGet Packages

```powershell
# SpeckitDemo.Data
dotnet add src/SpeckitDemo.Data/SpeckitDemo.Data.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.5
dotnet add src/SpeckitDemo.Data/SpeckitDemo.Data.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.5

# SpeckitDemo.DataAccess  
dotnet add src/SpeckitDemo.DataAccess/SpeckitDemo.DataAccess.csproj package Microsoft.EntityFrameworkCore --version 10.0.5

# SpeckitDemo.Application
dotnet add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj package FluentValidation --version 12.1.1
dotnet add src/SpeckitDemo.Application/SpeckitDemo.Application.csproj package FluentValidation.DependencyInjectionExtensions --version 12.1.1

# SpeckitDemo.Security
dotnet add src/SpeckitDemo.Security/SpeckitDemo.Security.csproj package BCrypt.Net-Next --version 4.1.0
dotnet add src/SpeckitDemo.Security/SpeckitDemo.Security.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.5
dotnet add src/SpeckitDemo.Security/SpeckitDemo.Security.csproj package System.IdentityModel.Tokens.Jwt --version 8.17.0

# SpeckitDemo.Api
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.5
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj package Asp.Versioning.Mvc --version 8.1.1
dotnet add src/SpeckitDemo.Api/SpeckitDemo.Api.csproj package Swashbuckle.AspNetCore --version 7.3.1

# SpeckitDemo.Tests
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj package Moq --version 4.20.72
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj package FluentAssertions --version 7.0.0
dotnet add tests/SpeckitDemo.Tests/SpeckitDemo.Tests.csproj package Microsoft.NET.Test.Sdk
```

---

## 5. Configure Secrets (Local Dev Only)

Create `src/SpeckitDemo.Api/appsettings.Development.json` (**this file is git-ignored**):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=speckitdemo.db"
  },
  "Jwt": {
    "Issuer": "SpeckitDemo",
    "Audience": "SpeckitDemo",
    "Key": "your-super-secret-signing-key-min-32-chars!!",
    "ExpiryHours": 1
  }
}
```

> **Security**: The `Jwt:Key` must be at least 32 characters (256 bits) for HS256. Never commit this file. Verify it is in `.gitignore`.

Add to `.gitignore`:
```
src/SpeckitDemo.Api/appsettings.Development.json
*.db
*.db-shm
*.db-wal
```

---

## 6. Generate Pre-Computed Password Hashes for Seed Data

Before creating the migration, compute the hash for the seed password `admin` once:

```powershell
# Run a quick C# script to get the BCrypt hash
dotnet script -e "Console.WriteLine(BCrypt.Net.BCrypt.HashPassword(\"admin\", 12));"
# OR write a temporary console app snippet and run it
```

Copy the output hash string (e.g., `$2a$12$...`) into `MemberMap.cs` `HasData()` seed entries **as string literals** — do not call `BCrypt.HashPassword()` inside the configuration; doing so generates new random salts on every `migrations add`.

---

## 7. Create and Apply Database Migration

```powershell
# Generate the initial migration (run from solution root)
dotnet ef migrations add InitialCreate --project src/SpeckitDemo.Data --startup-project src/SpeckitDemo.Api

# Apply migration to create the SQLite database (creates speckitdemo.db)
dotnet ef database update --project src/SpeckitDemo.Data --startup-project src/SpeckitDemo.Api
```

After this step, `speckitdemo.db` is created in `src/SpeckitDemo.Api/` with the `Users` table seeded with 3 users (alice, bob, charlie — all with password `admin`).

---

## 8. Build the Solution

```powershell
dotnet build
```

All 7 projects should build without warnings or errors.

---

## 9. Run the API

```powershell
dotnet run --project src/SpeckitDemo.Api
```

The API starts at `https://localhost:5001` (or as configured). Swagger UI is available at `https://localhost:5001/swagger`.

---

## 10. Test the Endpoints

### Login (get a token)

```powershell
$response = Invoke-RestMethod -Uri "https://localhost:5001/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"username":"alice","password":"admin"}'
$token = $response.data.token
Write-Host "Token: $token"
```

### Fetch User Details

```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/v1/users/me" `
    -Method GET `
    -Headers @{ Authorization = "Bearer $token" }
```

Expected response:
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

---

## 11. Run Unit Tests

```powershell
dotnet test tests/SpeckitDemo.Tests
```

All tests must pass. Target: 100% coverage of `SignInWorkflow` primary and failure paths.

---

## Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| `dotnet ef` not found | Global tool not installed | `dotnet tool install --global dotnet-ef --version 10.0.5` |
| `Jwt:Key` null reference on startup | Missing `appsettings.Development.json` | Create the file per step 5 |
| `UpdateData` migration created unexpectedly | `BCrypt.HashPassword()` called inside `HasData()` | Replace with pre-computed hash string literals |
| HTTP 401 on valid token | `ClockSkew` not set to zero | Set `ClockSkew = TimeSpan.Zero` in `TokenValidationParameters` |
| SQLite `speckitdemo.db` already exists with old schema | Stale database file | Delete `speckitdemo.db` and re-run `dotnet ef database update` |
