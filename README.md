# SpeckitDemo Access

Small ASP.NET Core API that signs a person in with a username and password and returns a signed bearer token. A second endpoint reads the profile for whoever that token belongs to.

Accounts live in a local SQLite file. Passwords are stored as BCrypt hashes. Tokens are HS256 and expire one hour after they are issued.

## Layout

```
src/SpeckitDemo.Api            HTTP host, Swagger, and the request pipeline
src/SpeckitDemo.Application     Sign-in and profile workflows
src/SpeckitDemo.Contracts       Request and response shapes
src/SpeckitDemo.Data            EF Core context, mapping, and migrations
src/SpeckitDemo.DataAccess      Account queries
src/SpeckitDemo.Security        Password hashing and token signing
tests/SpeckitDemo.Tests         Workflow and validation tests
```

## What you need

- .NET SDK 10
- The `dotnet-ef` tool at 10.0.5, already listed in `.config/dotnet-tools.json`

From the repository root:

```powershell
dotnet tool restore
```

## Local settings

`src/SpeckitDemo.Api/appsettings.Development.json` holds the signing key and is not committed. Create it if it is missing:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=speckitdemo.db"
  },
  "Jwt": {
    "Issuer": "SpeckitDemo",
    "Audience": "SpeckitDemo",
    "Key": "speckitdemo-dev-signing-key-change-before-release",
    "ExpiryHours": 1
  }
}
```

The key has to be at least 32 characters. Replace it before you point this at anything other than your own machine.

## Database

```powershell
dotnet ef database update --project src/SpeckitDemo.Data --startup-project src/SpeckitDemo.Api
```

That creates `speckitdemo.db` next to the API project and loads three accounts. Every one of them uses the password `admin`.

| Username | Name |
| --- | --- |
| mira | Mira Calder |
| nolan | Nolan Briggs |
| priya | Priya Raman |

## Run

```powershell
dotnet run --project src/SpeckitDemo.Api
```

The HTTP profile listens on `http://localhost:5022`. Swagger is at `http://localhost:5022/swagger` while the environment is Development.

Sign in:

```powershell
$response = Invoke-RestMethod -Uri "http://localhost:5022/api/v1/auth/login" `
    -Method POST `
    -ContentType "application/json" `
    -Body '{"username":"mira","password":"admin"}'
$token = $response.data.token
```

Read the profile:

```powershell
Invoke-RestMethod -Uri "http://localhost:5022/api/v1/users/me" `
    -Method GET `
    -Headers @{ Authorization = "Bearer $token" }
```

`POST /api/v1/auth/login` returns 400 when the body is incomplete, 401 when the username or password does not match, and 200 with `token`, `tokenType`, and `expiresAt` when it does. Unknown usernames and wrong passwords return the same message. `GET /api/v1/users/me` requires the bearer token and never includes the password hash.

## Tests

```powershell
dotnet test SpeckitDemo.sln
```
