# FootballPairs Source Tree Analysis

## Scope

This tree documents runtime-relevant folders and intentionally excludes generated build output such as `bin/` and `obj/`.

## Annotated Tree

```text
.
|- FootballPairs.sln                        # Solution entry point
|- README.md                               # User-facing setup and API guide
|- EXPLANATION.md                          # Deep architecture explanation
|- FootballPairs.Api/                      # ASP.NET Core host and HTTP boundary
|  |- Configuration/
|  |  `- ImportPathOptions.cs              # Allowed import root settings
|  |- Contracts/
|  |  |- Requests/                         # HTTP request bodies
|  |  `- Responses/                        # HTTP response DTOs
|  |- Controllers/                         # Route handlers for auth, CRUD, imports, analytics
|  |- Extensions/
|  |  `- AuthExtensions.cs                 # JWT auth and authorization setup
|  |- Middleware/
|  |  |- ExceptionHandlingMiddleware.cs    # Maps exceptions to ProblemDetails
|  |  `- RequestLoggingMiddleware.cs       # Sanitized request logging
|  |- Properties/
|  |  `- launchSettings.json               # Local development URLs
|  |- Program.cs                           # Runtime entry point
|  |- appsettings.json                     # Connection string, JWT, import roots
|  `- appsettings.Development.json         # Development overrides
|- FootballPairs.Application/              # Business rules and use-case orchestration
|  |- Analytics/                           # Played-time and common-match calculations
|  |- Auth/                                # Registration, login, token revocation
|  |- Common/Errors/                       # Stable error types and error codes
|  |- Import/                              # CSV import orchestration and validation
|  |- Logging/                             # Logging abstractions and fallback policy
|  |- Matches/                             # Match rules and CRUD
|  |- MatchRecords/                        # Interval rules and CRUD
|  |- Players/                             # Player rules and CRUD
|  `- Teams/                               # Team rules and CRUD
|- FootballPairs.Domain/                   # Core entities and shared invariants
|  |- Entities/
|  |  |- User.cs
|  |  |- Team.cs
|  |  |- Player.cs
|  |  |- Match.cs
|  |  |- MatchRecord.cs
|  |  |- RequestLog.cs
|  |  `- RevokedToken.cs
|  `- DomainLimits.cs                      # Default match end minute
|- FootballPairs.Infrastructure/           # EF Core, SQL Server, security, logging, CSV
|  |- Csv/
|  |  |- CsvParser.cs
|  |  |- DateParser.cs
|  |  `- HeaderMappingService.cs
|  |- Logging/
|  |  |- DbRequestLogSink.cs               # Database request log sink
|  |  |- FileRequestLogSink.cs             # Local file fallback sink
|  |  `- JsonLogDataSanitizer.cs           # Secret redaction
|  |- Persistence/
|  |  |- Configurations/                   # EF entity mappings
|  |  |- Migrations/                       # Schema evolution history
|  |  |- FootballPairsDbContext.cs         # DbSet definitions
|  |  `- *Repository.cs                    # Data access implementations
|  |- Security/
|  |  |- JwtTokenService.cs
|  |  `- PasswordHasher.cs
|  |- Sql/Triggers/
|  |  |- MatchRecords_NoOverlap.sql        # DB guard against overlapping intervals
|  |  `- RevokedTokens_DeleteExpiredAfter24Hours.sql
|  `- DependencyInjection.cs               # Composition root
|- docs/
|  `- postman/                             # Manual API exploration assets
|- samples/                                # CSV import samples
|- LocalDB/                                # Local database files
`- logs/                                   # Fallback request logs and audit files
```

## Critical Folders Summary

| Path | Why it matters | Notable files |
| --- | --- | --- |
| `FootballPairs.Api/Controllers` | Defines the public API surface and authorization rules. | `AuthController.cs`, `ImportController.cs`, `AnalyticsController.cs` |
| `FootballPairs.Api/Middleware` | Centralizes error formatting and request logging. | `ExceptionHandlingMiddleware.cs`, `RequestLoggingMiddleware.cs` |
| `FootballPairs.Application` | Holds the business logic that should remain stable when transport or storage changes. | `AuthService.cs`, `MatchService.cs`, `MatchRecordService.cs`, `AnalyticsService.cs` |
| `FootballPairs.Domain/Entities` | Describes persistent business objects. | `User.cs`, `Match.cs`, `MatchRecord.cs` |
| `FootballPairs.Infrastructure/Persistence` | Contains EF Core mappings, migrations, and repositories. | `FootballPairsDbContext.cs`, migration files, repository classes |
| `FootballPairs.Infrastructure/Logging` | Implements resilient request logging and sensitive-data redaction. | `DbRequestLogSink.cs`, `FileRequestLogSink.cs`, `JsonLogDataSanitizer.cs` |
| `FootballPairs.Infrastructure/Sql/Triggers` | Applies database-level protection beyond application validation. | `MatchRecords_NoOverlap.sql`, `RevokedTokens_DeleteExpiredAfter24Hours.sql` |
| `docs/postman` | Supports manual verification and onboarding. | `README.md`, `FootballPairs.postman_collection.json` |
| `samples` | Provides known-good CSV files for import flows. | `teams.csv`, `players.csv`, `matches.csv`, `records.csv` |

## Entry Points

- Runtime entry point: `FootballPairs.Api/Program.cs`
- DI composition root: `FootballPairs.Infrastructure/DependencyInjection.cs`
- Auth setup hook: `FootballPairs.Api/Extensions/AuthExtensions.cs`
- Database model root: `FootballPairs.Infrastructure/Persistence/FootballPairsDbContext.cs`

## File Organization Patterns

- API contracts stay isolated under `FootballPairs.Api/Contracts`, which keeps controllers thin.
- Each application feature uses a folder-per-feature structure with commands, DTOs, interfaces, and service logic grouped together.
- Infrastructure mirrors application abstractions with repository, security, logging, and parsing implementations.
- SQL triggers live beside migrations but outside EF configuration classes so database-only invariants remain explicit.
