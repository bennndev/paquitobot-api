# PaquitoBot API

Backend API for **PaquitoBot**, built with .NET following a hexagonal (ports and adapters) architecture.

## Solution Structure

| Project | Responsibility |
|---|---|
| `PaquitoBot.Api` | Controllers, endpoints and application configuration |
| `PaquitoBot.Application` | DTOs, use cases (commands and queries) and repository contracts |
| `PaquitoBot.Domain` | Domain entities and business rules |
| `PaquitoBot.Infrastructure` | Persistence, repository implementations and the Frappe HTTP client |
| `PaquitoBot.Infrastructure.Tests` | Unit tests for the Frappe integration |

## Tech Stack

- .NET and ASP.NET Core
- Entity Framework Core with PostgreSQL (hosted on Neon)

## Getting Started

1. Clone the repository and open `PaquitoBot.sln`.
2. Store the database connection string with .NET User Secrets. Never put credentials in `appsettings.json`.
3. Configure Frappe as described below and build the solution before running the API.

## Frappe Integration

The API validates the Frappe URL and credentials at startup. Set the following .NET User Secrets for local development and replace the placeholders with credentials for your Frappe service account.

```powershell
dotnet user-secrets set "Frappe:BaseUrl" "http://education.localhost:8000" --project PaquitoBot.Api
dotnet user-secrets set "Frappe:ApiKey" "<api-key>" --project PaquitoBot.Api
dotnet user-secrets set "Frappe:ApiSecret" "<api-secret>" --project PaquitoBot.Api
```

For deployments, use `Frappe__BaseUrl`, `Frappe__ApiKey` and `Frappe__ApiSecret` through your deployment configuration and secret store. Use HTTPS for remote instances. The optional `Frappe__TimeoutSeconds` defaults to 15 and covers both the response headers and body.

Send an API bearer token to `GET /health/frappe`. The endpoint returns HTTP 200 when Frappe accepts the configured service account and HTTP 503 when the check fails.

Application modules can inject `IFrappeClient` and call `GetPageAsync<T>` with a resource name, optional field names and filters. Use `NextOffset` for each subsequent page until it is null. Frappe can return fewer rows than requested, so the client advances by the actual row count. The client maps Frappe's snake_case fields to the properties of the requested DTO and reports normalized failures through `FrappeException.Failure`.

Use `IFrappeSyncRunService` to start a run and then record its success or failure with the number of processed rows. Each operation uses its own unit of work. When recording a cancellation after the caller's token has expired, pass a separate, bounded token for the final audit write.

Run the unit tests with `dotnet test PaquitoBot.sln`. The client tests use a simulated HTTP handler and require no Frappe or PostgreSQL server. [Frappe REST API](https://docs.frappe.io/framework/user/en/api/rest)

## Contributing

Work follows **Issue → Branch → Commits → Pull Request → Review → Merge**.

- Every task starts from a GitHub Issue, and every Pull Request references one.
- Branch from `development` using `<type>/<short-description>`, for example `feature/student-management`.
- Use [Conventional Commits](https://www.conventionalcommits.org/): `<type>(<scope>): <short-description>`, for example `feat(inf): implement generic repository`.
- Open Pull Requests against `development`. They need at least one approval and a successful build.
- Never commit credentials, connection strings or secrets.

### Commit Rules

Commits follow **Conventional Commits** for the message and **Atomic Commits** for what goes inside.

- **One commit per logical change, not per file.** A commit can include 1, 5 or 20 files as long as all of them serve the same goal.
- Before committing, ask: *if I revert this commit, do I revert a single logical change and leave the project in a coherent state?* If yes, it is well grouped.
- Keep together what only works together: a use case with its validator and handler, a migration with its designer and model snapshot, a controller with the contracts it needs.
- Do not mix unrelated changes in one commit, and do not split one change across commits so that the intermediate ones do not build.
- Prefer commits that compile and can be verified on their own.
- Review `git diff --staged` before every commit, because the staged files may include changes from another task.

Examples:

```text
feat(app): add student use cases
feat(api): add students controller
feat(db): add academic payment schema
refactor(app): migrate use cases to MediatR
```

The full rules (branch types, commit scopes, Issue and PR conventions, labels and Definition of Done) are in [CONTRIBUTING.md](CONTRIBUTING.md).
