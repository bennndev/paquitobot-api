# PaquitoBot API

Backend API for **PaquitoBot**, built with .NET following a hexagonal (ports and adapters) architecture.

## Entorno local de Frappe Education

El entorno reproducible para el equipo está documentado en
[`infra/frappe/README.md`](infra/frappe/README.md). Con Docker Desktop iniciado:

```powershell
powershell -ExecutionPolicy Bypass -File .\infra\frappe\setup.ps1
```

## Solution Structure

| Project | Responsibility |
|---|---|
| `PaquitoBot.Api` | Controllers, endpoints and application configuration |
| `PaquitoBot.Application` | DTOs, use cases (commands and queries) and repository contracts |
| `PaquitoBot.Domain` | Domain entities and business rules |
| `PaquitoBot.Infrastructure` | Persistence, repository implementations and the Frappe HTTP client |

## Tech Stack

- .NET and ASP.NET Core
- Entity Framework Core with PostgreSQL (hosted on Neon)

## Getting Started

1. Clone the repository and open `PaquitoBot.sln`.
2. Store the database connection string with .NET User Secrets. Never put credentials in `appsettings.json`.
3. Configure Frappe as described below and build the solution before running the API.

## Frappe Client

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

[Frappe REST API](https://docs.frappe.io/framework/user/en/api/rest)

## Frappe Synchronization

Two integrations with Frappe Education coexist. They use different credentials and serve different purposes.

| | Master-data sync | Per-student OAuth snapshot |
|---|---|---|
| Endpoints | `POST api/academic-sync`, `GET api/sync-runs/latest` | `GET api/frappe/connect`, `GET api/frappe/callback` |
| Who triggers it | An administrator (policy `Administrator`, role `Admin`) | A signed-in student (`connect` requires a JWT; `callback` is anonymous and validates the OAuth state) |
| Credentials | Frappe API key and secret, configuration section `Frappe` | OAuth client, configuration section `FrappeOAuth` |
| Scope | Academic master data for everybody | Only the data of the student who authorized |
| Entry point | `RunAcademicSyncCommand` | `CompleteFrappeConnectionCommand` and `FrappeOAuthService` |

### Master-data sync

`POST api/academic-sync` runs `RunAcademicSyncCommand`, which executes one synchronizer per entity (`PaquitoBot.Application/Services/Sync/`) fed by a Frappe source (`PaquitoBot.Infrastructure/Services/Frappe/`). Every source reads through the shared `IFrappeClient` (see [Frappe Client](#frappe-client)), which pages in a stable order and reports failures as `FrappeException`.

- **Dependency order.** Synchronizers run by `SyncOrder` tier, so every tier only references lower ones: `MasterData` (10: academic periods, programs, courses, instructors, students), `ProgramStructure` (20: program courses), `Sections` (30: course sections), `SectionDetails` (40: course schedules, program enrollments, section enrollments).
- **Idempotent.** Records are matched by `FrappeExternalId`; a record is created, updated or left unchanged, so running it again is safe.
- **Per-record failures.** A bad record (for example a parent that was not found locally) is reported in the response as a failed record with its external id and message, and does not stop the rest.
- **One `SyncRun` per entity.** A failure in one entity does not stop the others. Only one sync can run at a time; a second request gets a conflict.
- **Never deletes.** Local data is never removed, even if it no longer exists in Frappe.
- **Results.** The response lists processed, created, updated, unchanged and failed counts per entity. `GET api/sync-runs/latest` returns the latest run for each entity.

Configuration keys: `Frappe:BaseUrl`, `Frappe:ApiKey`, `Frappe:ApiSecret`. Do not set them by hand: `infra/frappe/run-api.ps1` loads them as environment variables (`Frappe__BaseUrl`, and so on) from the local Frappe environment described in [`infra/frappe/README.md`](infra/frappe/README.md).

### Per-student OAuth snapshot

A signed-in user calls `GET api/frappe/connect`, which returns the Frappe authorization URL. After the student authorizes, Frappe redirects to `GET api/frappe/callback`, which exchanges the code, checks that the Frappe account matches the PaquitoBot user, and imports that student's snapshot through the server script `paquitobot_student_snapshot`.

The script and how to install it are in [`integrations/frappe/README.md`](integrations/frappe/README.md). Set `FrappeOAuth:BaseUrl`, `FrappeOAuth:ClientId`, `FrappeOAuth:ClientSecret` and `FrappeOAuth:RedirectUri` in User Secrets; the redirect URI must match the Frappe OAuth Client exactly.

### Local configuration

The API will not start without these User Secrets (use placeholders below, never commit real values):

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<postgres-connection-string>" --project PaquitoBot.Api
dotnet user-secrets set "JWTVariable:Secret" "<random-string-of-at-least-32-bytes>" --project PaquitoBot.Api
dotnet user-secrets set "JWTVariable:Issuer" "<issuer>" --project PaquitoBot.Api
dotnet user-secrets set "JWTVariable:Audience" "<audience>" --project PaquitoBot.Api
```

### Gotchas

1. .NET on Windows cannot resolve `*.localhost` names. The API reaches Frappe through `http://localhost:8000`, while the browser uses `http://education.localhost:8000`. Use the `localhost` form for `Frappe:BaseUrl`.
2. Start the API with `infra/frappe/run-api.ps1`, not from an IDE run configuration. Otherwise the Frappe API key is not injected and Frappe answers 403.

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
