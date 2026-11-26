# Reporting smoke checklist (TEST environment only)

1. Start the API with a nonproduction PostgreSQL connection string, JWT config and Frappe settings.
2. Without JWT, `GET /api/reports/enrollments` must return 401.
3. With a `student` JWT, it must return 403.
4. With an admin JWT, the JSON reports should return 200 (empty lists are valid).
5. Excel versions return status 200, content-type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`, and a ZIP-signature xlsx (`PK`).
6. Exported Excel summary must match JSON counts and ratios.
7. `attendance?from=2026-11-30&to=2026-11-01` must return 400 (FluentValidation).
8. `finances/{studentId}` must match the existing `student-finances/{studentId}/statement` JSON for the same admin and asOf date.
9. Check cross-student access with a `student` JWT (both 403).
10. Rerun against 0 rows; file has a header and no invented data. No writes must occur.

### Standalone tests

`dotnet test PaquitoBot.Reports.Tests/PaquitoBot.Reports.Tests.csproj --configuration Release`

### Commit / PR rules

- Use `feature/academic-financial-reports`, based on the latest `origin/development`.
- Create a dedicated Issue before opening a PR, and link the PR with `Closes #<n>`.
- No changes to `development` directly; no credentials in Git.
- ClosedXML 0.105.1 is installed **only** in Infrastructure.
- CI must not be described as running these tests unless CI is separately updated to do so.
