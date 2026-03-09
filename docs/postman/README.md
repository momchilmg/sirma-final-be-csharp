# FootballPairs Postman Quick Guide

## Base Setup
- Base URL: `https://localhost:7182` (or your local API URL)
- Header for authenticated requests: `Authorization: Bearer {{accessToken}}`
- Content type for JSON endpoints: `Content-Type: application/json`
- CORS is globally allow-all in API (`AllowAnyOrigin/AllowAnyMethod/AllowAnyHeader`) for frontend integration.

## Auth Flow
1. `POST /api/auth/register`
2. `POST /api/auth/login`
3. Store `accessToken` from login response and send it as bearer token
4. Optional check: `GET /api/auth/me`
5. Logout when needed: `POST /api/auth/logout` and revoke current token

### Register Sample
`POST /api/auth/register`
```json
{
  "username": "admin1",
  "password": "StrongPass123!"
}
```

### Login Sample
`POST /api/auth/login`
```json
{
  "username": "admin1",
  "password": "StrongPass123!"
}
```

## Endpoints

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/auth/admin-check`

### Teams
- `POST /api/teams`
- `GET /api/teams`
- `GET /api/teams/{id}`
- `PUT /api/teams/{id}`
- `DELETE /api/teams/{id}`

### Players
- `POST /api/players`
- `GET /api/players`
- `GET /api/players/{id}`
- `PUT /api/players/{id}`
- `DELETE /api/players/{id}`

### Matches
- `POST /api/matches`
- `GET /api/matches`
- `GET /api/matches/{id}`
- `PUT /api/matches/{id}`
- `DELETE /api/matches/{id}`
- Rule: a team can participate in only one match per calendar day (create/update/import all enforce this).

### Match Records
- `POST /api/match-records`
- `GET /api/match-records`
- `GET /api/match-records/{id}`
- `PUT /api/match-records/{id}`
- `DELETE /api/match-records/{id}`
- Rule: one player can have only one record per match (`MatchId` + `PlayerId`).

### Import
- `POST /api/import/teams`
- `POST /api/import/players`
- `POST /api/import/matches`
- `POST /api/import/match-records`

### Analytics
- `GET /api/analytics/players/{playerAId}/with/{playerBId}/played-time`
- `GET /api/analytics/players/{playerAId}/with/{playerBId}/common-matches`

## Create/Update Payload Samples

### Team
```json
{
  "name": "Germany",
  "managerFullName": "Julian Nagelsmann",
  "group": "A"
}
```

### Player
```json
{
  "teamNumber": 10,
  "position": "MF",
  "fullName": "Example Midfielder",
  "teamId": 1
}
```

### Match
```json
{
  "matchDate": "2024-06-14T00:00:00Z",
  "homeTeamId": 1,
  "awayTeamId": 2,
  "score": "2-1"
}
```

### Match Record
```json
{
  "matchId": 1,
  "playerId": 10,
  "fromMinute": 0,
  "toMinute": 90
}
```

## Import Instructions

Use `multipart/form-data` with exactly one source:
- `file`: CSV upload
- `path`: server-local file path (example: `D:\Sirma\JS\Final Exam\be\code\samples\teams.csv`)

If both or neither are provided, API returns `400`.
CSV source size is limited to `100MB` per request.
`path` must resolve under configured roots from `ImportPaths:AllowedRoots` (default: `samples` under app content root).

### Example form-data keys
- `file` = `<uploaded csv file>`
- or `path` = `D:\Sirma\JS\Final Exam\be\code\samples\players.csv`

### CSV Samples
- `samples/teams.csv` required columns: `Name`, `ManagerFullName`, `Group`
- `samples/players.csv` required columns: `FullName`, `Position`, `TeamNumber`, `TeamID`
- `samples/matches.csv` required columns: `ATeamID`, `BTeamID`, `Date`, `Score`
- `samples/records.csv` required columns: `PlayerID`, `MatchID`, `fromMinutes`, `toMinutes` (`NULL` allowed for `toMinutes`)

## Response Notes
- Validation errors: `application/problem+json` (`400`)
- Auth/role errors: `401`/`403` `application/problem+json`
- Not found: `404` `application/problem+json`
- Conflicts: `409` `application/problem+json`
- Match conflicts return `409` when either team already has a match on the same calendar date.
- Match-record conflicts return `409` when `(MatchId, PlayerId)` already exists.
- After `POST /api/auth/logout`, the same token is revoked and should return `401` on next protected request.
