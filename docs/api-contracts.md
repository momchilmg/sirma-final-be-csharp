# FootballPairs API Contracts

## Conventions

- Base HTTPS URL during local development: `https://localhost:7182`
- Base HTTP URL during local development: `http://localhost:5140`
- Auth scheme: `Authorization: Bearer <accessToken>`
- Default response format: JSON
- Error format: `application/problem+json`

All endpoints except `POST /api/auth/register` and `POST /api/auth/login` require authentication.

## Common Error Model

Typical error fields:

- `type`
- `title`
- `status`
- `instance`
- `traceId`
- `errorCode`

Common error codes:

| HTTP status | `errorCode` | Typical cause |
| --- | --- | --- |
| `400` | `ValidationFailed` | Invalid payload, invalid query values, invalid import source |
| `401` | `Unauthenticated` | Missing, invalid, expired, or revoked JWT |
| `403` | `Forbidden` | Authenticated caller lacks `admin` role |
| `404` | `NotFound` | Requested entity does not exist |
| `409` | `Conflict` | Business rule conflict such as duplicate match record or schedule collision |
| `500` | `UnhandledError` | Unexpected server error |

Example error response:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "instance": "/api/players",
  "traceId": "00-00000000000000000000000000000000-0000000000000000-00",
  "errorCode": "ValidationFailed"
}
```

## Authentication

### Endpoints

| Method | Path | Auth | Response |
| --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Anonymous | `201 Created`, `RegisterResponse` |
| `POST` | `/api/auth/login` | Anonymous | `200 OK`, `LoginResponse` |
| `POST` | `/api/auth/logout` | Authenticated | `204 No Content` |
| `GET` | `/api/auth/me` | Authenticated | `200 OK`, `MeResponse` |
| `GET` | `/api/auth/admin-check` | Admin | `200 OK`, `{ "allowed": true }` |

### Request Schemas

#### `RegisterRequest` and `LoginRequest`

| Field | Type | Rules |
| --- | --- | --- |
| `username` | `string` | required, length `3..64` |
| `password` | `string` | required, length `8..128` |

### Response Schemas

#### `RegisterResponse`

| Field | Type |
| --- | --- |
| `id` | `guid` |
| `username` | `string` |
| `role` | `string` |
| `createdAt` | `date-time` |

#### `LoginResponse`

| Field | Type |
| --- | --- |
| `accessToken` | `string` |

#### `MeResponse`

| Field | Type |
| --- | --- |
| `userId` | `string \| null` |
| `role` | `string \| null` |
| `username` | `string \| null` |

### Example

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin1",
  "password": "StrongPass123!"
}
```

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

## Teams

### Endpoints

| Method | Path | Auth | Notes |
| --- | --- | --- | --- |
| `POST` | `/api/teams` | Admin | Create team |
| `GET` | `/api/teams` | Authenticated | List teams |
| `GET` | `/api/teams/{id}` | Authenticated | Get team by ID |
| `PUT` | `/api/teams/{id}` | Admin | Update team |
| `DELETE` | `/api/teams/{id}` | Admin | Delete team |

### Body Schema

`CreateTeamRequest` and `UpdateTeamRequest`

| Field | Type | Rules |
| --- | --- | --- |
| `name` | `string` | required, length `1..100` |
| `managerFullName` | `string` | required, length `1..150` |
| `group` | `string` | required, length `1..20` |

### Response Schema

`TeamResponse`

| Field | Type |
| --- | --- |
| `id` | `int` |
| `name` | `string` |
| `managerFullName` | `string` |
| `group` | `string` |

## Players

### Endpoints

| Method | Path | Auth | Notes |
| --- | --- | --- | --- |
| `POST` | `/api/players` | Admin | Create player |
| `GET` | `/api/players` | Authenticated | List players |
| `GET` | `/api/players/{id}` | Authenticated | Get player by ID |
| `PUT` | `/api/players/{id}` | Admin | Update player |
| `DELETE` | `/api/players/{id}` | Admin | Delete player |

### Body Schema

`CreatePlayerRequest` and `UpdatePlayerRequest`

| Field | Type | Rules |
| --- | --- | --- |
| `teamNumber` | `int` | required, `>= 1` |
| `position` | `string` | required, length `1..10` |
| `fullName` | `string` | required, length `1..150` |
| `teamId` | `int` | required, `>= 1` |

### Response Schema

`PlayerResponse`

| Field | Type |
| --- | --- |
| `id` | `int` |
| `teamNumber` | `int` |
| `position` | `string` |
| `fullName` | `string` |
| `teamId` | `int` |

## Matches

### Endpoints

| Method | Path | Auth | Notes |
| --- | --- | --- | --- |
| `POST` | `/api/matches` | Admin | Create match |
| `GET` | `/api/matches` | Authenticated | List matches |
| `GET` | `/api/matches/{id}` | Authenticated | Get match by ID |
| `PUT` | `/api/matches/{id}` | Admin | Update match |
| `DELETE` | `/api/matches/{id}` | Admin | Delete match |

### Body Schema

`CreateMatchRequest` and `UpdateMatchRequest`

| Field | Type | Rules |
| --- | --- | --- |
| `matchDate` | `date-time` | required |
| `homeTeamId` | `int` | required, `>= 1` |
| `awayTeamId` | `int` | required, `>= 1`, must differ from home team |
| `score` | `string` | required, length `3..20`, validated by score regex |

Accepted score examples:

- `2-1`
- `0(3)-0(5)`

### Response Schema

`MatchResponse`

| Field | Type |
| --- | --- |
| `id` | `int` |
| `matchDate` | `date-time` |
| `homeTeamId` | `int` |
| `awayTeamId` | `int` |
| `score` | `string` |
| `endMinute` | `int` |

### Important Conflict Rule

The API returns `409 Conflict` if either participating team already has another match on the same calendar date.

## Match Records

### Endpoints

| Method | Path | Auth | Notes |
| --- | --- | --- | --- |
| `POST` | `/api/match-records` | Admin | Create participation interval |
| `GET` | `/api/match-records` | Authenticated | Optional `matchId` filter |
| `GET` | `/api/match-records/{id}` | Authenticated | Get record by ID |
| `PUT` | `/api/match-records/{id}` | Admin | Update record |
| `DELETE` | `/api/match-records/{id}` | Admin | Delete record |

### Body Schema

`CreateMatchRecordRequest` and `UpdateMatchRecordRequest`

| Field | Type | Rules |
| --- | --- | --- |
| `matchId` | `int` | required, `>= 1` |
| `playerId` | `int` | required, `>= 1` |
| `fromMinute` | `int` | required, `>= 0` |
| `toMinute` | `int \| null` | optional, `>= 0`, must be greater than `fromMinute` when present |

### Response Schema

`MatchRecordResponse`

| Field | Type |
| --- | --- |
| `id` | `int` |
| `matchId` | `int` |
| `playerId` | `int` |
| `fromMinute` | `int` |
| `toMinute` | `int \| null` |

### Important Conflict Rules

- A player must belong to one of the match teams.
- Only one match record per `(MatchId, PlayerId)` is allowed.
- The database trigger rejects overlapping intervals for the same player in the same match.

## Imports

### Endpoints

| Method | Path | Auth | Response |
| --- | --- | --- | --- |
| `POST` | `/api/import/teams` | Admin | `ImportSummaryResponse` |
| `POST` | `/api/import/players` | Admin | `ImportSummaryResponse` |
| `POST` | `/api/import/matches` | Admin | `ImportSummaryResponse` |
| `POST` | `/api/import/match-records` | Admin | `ImportSummaryResponse` |

### Request Contract

All import endpoints consume `multipart/form-data` with exactly one of these fields:

| Field | Type | Rules |
| --- | --- | --- |
| `file` | file upload | optional, mutually exclusive with `path`, max `100 MB` |
| `path` | `string` | optional, mutually exclusive with `file`, must resolve under an allowed root |

### Response Schema

`ImportSummaryResponse`

| Field | Type |
| --- | --- |
| `entity` | `string` |
| `createdCount` | `int` |

### CSV Notes

- Team headers: `Name`, `ManagerFullName`, `Group`
- Player headers: `FullName`, `Position`, `TeamNumber`, `TeamId`
- Match headers: `HomeTeamId`, `AwayTeamId`, `MatchDate`, `Score`
- Match-record headers: `PlayerId`, `MatchId`, `FromMinute`, `ToMinute`

Alias support is built into the import layer, so legacy column names such as `ATeamID`, `BTeamID`, or `TeamID` still work.

## Analytics

### Endpoints

| Method | Path | Auth | Query parameters |
| --- | --- | --- | --- |
| `GET` | `/api/analytics/players/{playerAId}/with/{playerBId}/played-time` | Authenticated | `fromDate`, `toDate`, `matchId` |
| `GET` | `/api/analytics/players/{playerAId}/with/{playerBId}/common-matches` | Authenticated | `fromDate`, `toDate` |

### Response Schemas

`PlayedTimeResponse`

| Field | Type |
| --- | --- |
| `minutesTogether` | `int` |

`CommonMatchItemResponse`

| Field | Type |
| --- | --- |
| `matchId` | `int` |
| `matchDate` | `date-time` |
| `homeTeamId` | `int` |
| `awayTeamId` | `int` |
| `minutesTogether` | `int` |

`CommonMatchesResponse`

| Field | Type |
| --- | --- |
| `matches` | array of `CommonMatchItemResponse` |
| `totalMinutesTogether` | `int` |

### Example

```http
GET /api/analytics/players/1/with/2/common-matches?fromDate=2024-06-01&toDate=2024-07-01
Authorization: Bearer <token>
```

```json
{
  "matches": [
    {
      "matchId": 3,
      "matchDate": "2024-06-14T00:00:00",
      "homeTeamId": 1,
      "awayTeamId": 2,
      "minutesTogether": 45
    }
  ],
  "totalMinutesTogether": 45
}
```
