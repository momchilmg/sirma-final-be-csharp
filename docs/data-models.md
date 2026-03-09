# FootballPairs Data Models

## Overview

FootballPairs persists authentication, football domain data, analytics inputs, request audit logs, and revoked JWT token identifiers in a SQL Server database managed through EF Core migrations.

## Entity Inventory

### Users

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `uniqueidentifier` | primary key |
| `Username` | `nvarchar(64)` | required, unique |
| `PasswordHash` | `varbinary(max)` | required |
| `PasswordSalt` | `varbinary(max)` | required |
| `Iterations` | `int` | PBKDF2 iteration count |
| `Role` | `nvarchar(16)` | required |
| `CreatedAt` | `datetime` | required |
| `LastLoginAt` | `datetime null` | optional |

### Teams

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `Name` | `nvarchar(100)` | required |
| `ManagerFullName` | `nvarchar(150)` | required |
| `Group` | `nvarchar(20)` | required |

### Players

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `TeamNumber` | `int` | required |
| `Position` | `nvarchar(10)` | required |
| `FullName` | `nvarchar(150)` | required |
| `TeamId` | `int` | required FK to `Teams` |

### Matches

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `MatchDate` | `datetime` | required |
| `HomeTeamId` | `int` | required FK to `Teams` |
| `AwayTeamId` | `int` | required FK to `Teams` |
| `Score` | `nvarchar(20)` | required, regex-validated in application layer |
| `EndMinute` | `int` | required, defaults to `90` |

### MatchRecords

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `MatchId` | `int` | required FK to `Matches` |
| `PlayerId` | `int` | required FK to `Players` |
| `FromMinute` | `int` | required |
| `ToMinute` | `int null` | optional, `null` means match end |

### RequestLogs

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `Date` | `datetime` | required UTC timestamp |
| `ErrorCode` | `int` | stored HTTP status code |
| `Username` | `nvarchar(64) null` | optional user identity |
| `Data` | `nvarchar(max)` | sanitized JSON payload |

### RevokedTokens

| Field | Type | Notes |
| --- | --- | --- |
| `Id` | `int` | primary key, identity |
| `Jti` | `nvarchar(64)` | required, unique |
| `ExpiresAtUtc` | `datetime` | token expiry |
| `RevokedAtUtc` | `datetime` | revocation timestamp |

## Relationships

| From | To | Type | Notes |
| --- | --- | --- | --- |
| `Teams` | `Players` | one-to-many | a team owns many players |
| `Matches` | `Teams` | many-to-one twice | home and away team references |
| `MatchRecords` | `Matches` | many-to-one | delete restricted |
| `MatchRecords` | `Players` | many-to-one | delete restricted |

## Important Constraints

- `Users.Username` has a unique index.
- `RevokedTokens.Jti` has a unique index.
- `MatchRecords` has a unique index on `(MatchId, PlayerId)`.
- `MatchRecords` is mapped with trigger `TR_MatchRecords_NoOverlap`.
- `RevokedTokens` is mapped with trigger `TR_RevokedTokens_DeleteExpiredAfter24Hours`.

## Business Rules Enforced Above the Database

- The first registered user receives the `admin` role.
- Home and away team IDs must be different.
- Match scores must match supported formats.
- A team can appear in only one match per calendar date.
- A match record player must belong to one of the two match teams.
- `FromMinute` must be less than the effective end minute.
- `FromMinute` and `ToMinute` ranges are validated before persistence.

## Migration History

| Migration | Purpose |
| --- | --- |
| `20260301003218_InitialCreate` | Base schema |
| `20260301015228_AddTeamsAndPlayers` | Team and player support |
| `20260301022218_AddMatches` | Match storage |
| `20260301090448_AddMatchRecords` | Match participation intervals |
| `20260301101316_AddRequestLogs` | Persistent request logging |
| `20260306082255_AddRevokedTokens` | Token revocation storage |
| `20260306100403_EnforceSingleMatchRecordPerPlayer` | Unique `(MatchId, PlayerId)` rule |
| `20260307201058_AddRevokedTokensCleanupTrigger` | Automatic cleanup trigger for old revoked tokens |

## Schema Notes for Analytics

- Pair analytics depend on `MatchRecords` and `Matches`.
- Overlap math uses half-open intervals.
- When `ToMinute` is `null`, calculations use `Matches.EndMinute` or the domain default of `90`.
- Common-match analytics group records by `MatchId` and aggregate overlap per match.
