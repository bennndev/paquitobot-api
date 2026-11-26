# Report query translation regression — 2026-11-26

## Reported incident

In Neon TEST, the three authenticated administrative report GET endpoints returned 500.
Existing administrative CRUD endpoints returned 200, and a direct SQL aggregation of
`program_enrollments` returned 3 records in 2 program/period groups.

The .NET exception was `System.InvalidOperationException: The LINQ expression ... could not be translated`.
It originated inside `ReportReadRepository`, not authentication or ClosedXML.

## Implementation

- Keep filters, grouping, counts, `DISTINCT`, averages, sorting and `Take(5001)` on PostgreSQL.
- Project the EF query into SQL-compatible anonymous aggregate shapes.
- Convert the bounded materialized aggregate rows into application DTOs afterward.
- Normalize assessment scores before grouping to avoid a navigation join inside `Average`.
- No changes to database schema, controllers, Frappe synchronizers or financial ledger.

## Validation checklist (TEST only)

1. Stop the local API before rebuilding.
2. `dotnet build PaquitoBot.sln -c Release`
3. `dotnet test PaquitoBot.sln -c Release --no-build`
4. Start API with User Secrets pointing to Neon `test-reportes` (never production).
5. Obtain unexpired Admin and student JWTs for local TEST accounts.
6. `powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\pruebas-reportes\smoke-reportes.ps1 -BaseUrl http://localhost:5114 -ExpectedEnrollmentTotal 3`
7. Confirm all three JSON reports and XLSX downloads return 200; JWT access controls 401/403, invalid dates 400.
8. Review attendance and evaluation aggregates if the test branch has nonempty data.
9. Publish commits to the PR only after live regression checks pass.

Build and unit-test results do not independently prove that relational LINQ translation works.
Record the successful live HTTP check separately in PR #48; do not claim it passed until run.
