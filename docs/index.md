# FootballPairs Documentation Index

## Project Overview

- Type: monolith backend solution
- Primary language: C#
- Runtime: .NET 8
- Architecture: layered API, application, domain, and infrastructure projects
- Primary entry point: `FootballPairs.Api/Program.cs`

## Quick Reference

- Tech stack: ASP.NET Core Web API, EF Core 8, SQL Server LocalDB, JWT Bearer auth
- Public endpoints: register and login
- Admin-only areas: create, update, delete, and import endpoints
- Read-only authenticated areas: list, get-by-id, and analytics endpoints
- Manual verification assets: Postman collection and sample CSV files

## Documentation

- [Project Overview](./project-overview.md)
- [Architecture](./architecture.md)
- [Source Tree Analysis](./source-tree-analysis.md)
- [Development Guide](./development-guide.md)
- [API Contracts](./api-contracts.md)
- [Data Models](./data-models.md)

## Existing Documentation

- [Root README](../README.md) - Setup, endpoint walkthrough, and troubleshooting guide.
- [Detailed Explanation](../EXPLANATION.md) - Deep code-oriented explanation of layers and files.
- [Postman Quick Guide](./postman/README.md) - Manual request flow and payload examples.
- [Postman Collection](./postman/FootballPairs.postman_collection.json) - Executable request set for local testing.

## Getting Started

1. Read [Project Overview](./project-overview.md) for the system shape and the typical workflow.
2. Use [Development Guide](./development-guide.md) to restore, migrate, and run the API.
3. Use [API Contracts](./api-contracts.md) and the Postman assets to exercise the endpoints.
4. Use [Data Models](./data-models.md) and [Architecture](./architecture.md) when planning changes to persistence or business rules.
