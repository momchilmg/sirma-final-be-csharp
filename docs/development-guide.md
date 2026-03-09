# FootballPairs Development Guide

## Prerequisites

- .NET 8 SDK
- SQL Server LocalDB or another reachable SQL Server instance
- `dotnet-ef` CLI tool

Install the EF Core CLI if it is not already available:

```powershell
dotnet tool install --global dotnet-ef
```

## Solution Structure

The runtime solution includes four projects:

- `FootballPairs.Api`
- `FootballPairs.Application`
- `FootballPairs.Domain`
- `FootballPairs.Infrastructure`

List them with:

```powershell
dotnet sln FootballPairs.sln list
```

## Restore and Build

Run restore and build from the repository root:

```powershell
dotnet restore
dotnet build FootballPairs.sln
```

## Database Setup

The default connection string points to LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FootballPairsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

If you want to use LocalDB, make sure the instance exists and is running:

```powershell
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

Apply schema migrations:

```powershell
dotnet ef database update -p FootballPairs.Infrastructure -s FootballPairs.Api
```

## Runtime Configuration

Main settings live in `FootballPairs.Api/appsettings.json`.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | Database target |
| `Jwt:Key` | JWT signing key |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
| `Jwt:UsernameClaimType` | Name claim mapping |
| `Jwt:RoleClaimType` | Role claim mapping |
| `ImportPaths:AllowedRoots` | Server-local path roots for CSV import mode |

Important notes:

- The checked-in JWT key is explicitly for development only.
- If `Jwt:Key` is missing, startup fails immediately.
- Relative allowed import roots resolve from the API content root.

## Run the API

Use the API project directly:

```powershell
dotnet run --project FootballPairs.Api/FootballPairs.Api.csproj
```

Local launch profiles define:

- HTTPS: `https://localhost:7182`
- HTTP: `http://localhost:5140`

The launch profile opens `api/auth/me` by default.

## Common Development Tasks

### Register and log in

1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. Save the returned JWT
4. Add `Authorization: Bearer <token>` to protected requests

### Seed data through imports

Use the sample CSVs in `samples/`:

- `samples/teams.csv`
- `samples/players.csv`
- `samples/matches.csv`
- `samples/records.csv`

The import API accepts either:

- `file` for uploaded CSV content
- `path` for a server-local file under an allowed root

Exactly one source must be provided.

### Explore the API manually

Use the existing Postman assets:

- [`docs/postman/README.md`](./postman/README.md)
- [`docs/postman/FootballPairs.postman_collection.json`](./postman/FootballPairs.postman_collection.json)

## Verification Approach

This repository currently has no automated test project. The practical verification loop is:

1. Build the solution.
2. Apply migrations.
3. Run the API.
4. Exercise the Postman collection or targeted requests.
5. Check database-backed logs or fallback log files if needed.

## Logs and Troubleshooting

- Request logs can land in the database or in `logs/requests-yyyy-MM-dd.log`.
- Logging failure audits are written to `logs/logging-failures-yyyy-MM-dd.log`.
- If logout succeeds, the same token should fail on later protected requests.
- If a migration fails on unique match-record enforcement, inspect data for duplicate `(MatchId, PlayerId)` pairs before rerunning.
- If match-record inserts fail at the database level, inspect the overlap trigger behavior and the effective end-minute rules.

## Missing Operational Artifacts

The repository does not currently include:

- an automated test suite
- containerization files
- CI/CD workflows
- deployment manifests

If those capabilities are added later, extend this guide and create a dedicated deployment guide.
