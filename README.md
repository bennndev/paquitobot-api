# PaquitoBot API

Backend API for **PaquitoBot**, built with .NET following a hexagonal (ports and adapters) architecture.

## Solution Structure

| Project | Responsibility |
|---|---|
| `PaquitoBot.Api` | Controllers, endpoints and application configuration |
| `PaquitoBot.Application` | DTOs, use cases (commands and queries) and repository contracts |
| `PaquitoBot.Domain` | Domain entities and business rules |
| `PaquitoBot.Infrastructure` | Persistence and repository implementations |

## Tech Stack

- .NET and ASP.NET Core
- Entity Framework Core with PostgreSQL (hosted on Neon)

## Getting Started

1. Clone the repository and open `PaquitoBot.sln`.
2. Store the database connection string with .NET User Secrets. Never put credentials in `appsettings.json`.
3. Build the solution before running the API.

## Contributing

Work follows **Issue → Branch → Commits → Pull Request → Review → Merge**.

- Every task starts from a GitHub Issue, and every Pull Request references one.
- Branch from `development` using `<type>/<short-description>`, for example `feature/student-management`.
- Use [Conventional Commits](https://www.conventionalcommits.org/): `<type>(<scope>): <short-description>`, for example `feat(inf): implement generic repository`.
- Open Pull Requests against `development`. They need at least one approval and a successful build.
- Never commit credentials, connection strings or secrets.

The full rules (branch types, commit scopes, Issue and PR conventions, labels and Definition of Done) are in [CONTRIBUTING.md](CONTRIBUTING.md).
