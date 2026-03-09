# FootballPairs Architecture

## Executive Summary

FootballPairs is a layered backend application built as a single ASP.NET Core host over three supporting class libraries. The API layer handles transport and authorization, the application layer enforces domain rules, the domain layer defines core entities, and the infrastructure layer handles database access, token generation, CSV parsing, and logging.

## Architecture Pattern

- Pattern: layered backend architecture
- Deployment shape: single API service backed by SQL Server
- Repository shape: monolith solution with four runtime projects
- Primary style: service-oriented application layer over EF Core repositories

This pattern works well here because the project has strong validation logic, several cross-cutting policies, and a clear need to isolate persistence details from business rules.

## Layer Responsibilities

### API Layer

`FootballPairs.Api` owns:

- controller routing and request model binding
- JWT authentication and fallback authorization policy
- CORS configuration
- exception-to-ProblemDetails mapping
- request logging at the HTTP boundary

### Application Layer

`FootballPairs.Application` owns:

- business validation for CRUD operations
- auth rules such as first-user-becomes-admin
- import orchestration and batching
- analytics calculations over player overlap intervals
- stable application error types and error codes

### Domain Layer

`FootballPairs.Domain` owns:

- persistent entities
- shared invariants such as the default match end minute

### Infrastructure Layer

`FootballPairs.Infrastructure` owns:

- EF Core DbContext, mappings, repositories, and migrations
- password hashing and JWT token generation
- token revocation persistence
- CSV parsing, header alias mapping, and date parsing
- database-backed request logging plus file fallback
- SQL trigger scripts for database-level safeguards

## Request Lifecycle

1. `Program.cs` builds the service container and middleware pipeline.
2. `RequestLoggingMiddleware` wraps the request and logs sanitized metadata after execution.
3. `ExceptionHandlingMiddleware` converts known exceptions into consistent `application/problem+json` responses.
4. Authentication and authorization run before controller actions.
5. Controllers map HTTP payloads to application commands and call feature services.
6. Application services validate business rules and call repositories or helper services.
7. Repositories use EF Core against `FootballPairsDbContext`.
8. Controllers map application DTOs into API response contracts.

## Technology Decisions

| Category | Selected technology | Why it fits this codebase |
| --- | --- | --- |
| Web host | ASP.NET Core Web API | Simple controller routing and middleware support |
| Data access | EF Core 8.0.24 | Strong fit for a small relational backend with migrations |
| Database | SQL Server / LocalDB | Matches the exam-style local development setup |
| Authentication | JWT Bearer | Simple stateless auth with role claims |
| Password hashing | PBKDF2-SHA256 | Reasonable built-in choice for password storage |
| Import pipeline | Custom CSV parser | Gives full control over aliases, row validation, and error messages |
| Logging | DB primary plus file fallback | Preserves audit data when the database is temporarily unavailable |

## API Design

- Route prefixing is resource-oriented: `/api/auth`, `/api/teams`, `/api/players`, `/api/matches`, `/api/match-records`, `/api/import`, `/api/analytics`.
- Public access is limited to register and login.
- Read and analytics endpoints require authentication.
- Write and import endpoints require the `admin` role.
- Controllers stay thin; almost all business rules live below the API layer.

## Data Architecture

The main transactional tables are `Users`, `Teams`, `Players`, `Matches`, `MatchRecords`, `RequestLogs`, and `RevokedTokens`.

Key invariants:

- `Users.Username` is unique.
- `RevokedTokens.Jti` is unique.
- `MatchRecords` uses a unique index on `(MatchId, PlayerId)`.
- `TR_MatchRecords_NoOverlap` rejects overlapping intervals for the same player in the same match.
- Application validation prevents a team from appearing in more than one match per calendar date.

## Security Architecture

- JWT configuration is loaded from `appsettings.json`.
- Tokens include a `jti` claim and are checked against the revoked-token store on every authenticated request.
- Logout persists the token identifier and treats duplicate revocation as idempotent.
- Request logs are sanitized to redact passwords, tokens, cookies, API keys, and other secret-like fields.
- Import path mode resolves paths under configured allowed roots and rejects reparse points to reduce traversal risk.

## Logging and Resilience

- Primary request logs are written to the database via `DbRequestLogSink`.
- If the primary sink times out or the database is unavailable, `RequestLogWriter` falls back to daily local log files.
- If even fallback logging fails, the middleware writes a compact failure-audit entry to `logs/logging-failures-yyyy-MM-dd.log`.
- File logging uses a mutex plus retries to support concurrent writers.

## Operational Considerations

- CORS is configured as allow-all in `Program.cs`, which is acceptable for development but not a production default.
- The JWT signing key currently lives in source-controlled configuration and is explicitly marked as insecure for real production use.
- The repository does not include Docker, CI pipelines, or deployment manifests.
- The project includes Postman assets and CSV samples for manual verification.

## Testing Strategy

- No automated test project is present in the solution.
- The practical verification path is manual:
  - build the solution
  - run database migrations
  - start the API
  - exercise endpoints through Postman or another HTTP client

## Architecture Risks

- The lack of automated tests increases regression risk for validation-heavy services.
- The permissive CORS policy and hardcoded development JWT key should be treated as development-only settings.
- Without Swagger or OpenAPI generation, API discoverability depends on the documentation set and Postman collection staying current.
