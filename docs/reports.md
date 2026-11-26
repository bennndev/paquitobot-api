# Read-only academic and financial reports

All endpoints require an **Admin JWT**. Reports never edit data. This feature intentionally does not change Domain, the database schema, Frappe synchronizers, or the finance ledger.

## Scope and metrics

- Enrollment summary by program / academic period: enrollment records and distinct students. A student with multiple program enrollments appears in multiple groups but only once per group.
- Attendance summary by section: counts of `present`, `absent`, `late`, and `excused` (`on leave` and `leave` count as excused); anything else is `other`. Attendance percentage is `(present + late)/(present + late + absent)*100`, excluding excused/unknown statuses. A null percentage means no relevant records.
- Assessment summary by section: number of graded results and mean *percentage score*, calculated from `grade / maximum_score * 100` for evaluations with a positive maximum score. Unscored or invalid assessments are excluded. This is **not** a weighted course grade.
- Individual financial statement: reuse the exact MediatR query from Issue #41; do not recalculate payments, balances or statuses.

Program filtering for attendance and assessment means the section's course is included in that program's `ProgramCourse` catalog, which is not evidence of enrollment in the section. Academic period filters use `CourseSection.AcademicPeriodId`.

All reports are based on current local PostgreSQL data, not live Frappe. They are snapshots, not accounting-certified statements. Finance may be stale until Frappe synchronization is verified.

## Limits and security

- Admin-only; no student-wide data is exposed to student JWTs.
- Aggregates use `AsNoTracking`, group in PostgreSQL and cap results at **5,000 groups**. Above the limit a conflict is returned rather than silently truncating.
- XLSX files are generated with ClosedXML **only in Infrastructure**, have typed numeric cells, safe fixed worksheet names, filters and frozen headers.
- XLSX exports are created in memory (no filesystem storage); no secrets, card data, or connection strings are exported.
- Neither SQL migrations nor production writes are needed.

## Endpoints

- `GET /api/reports/enrollments?programId=&academicPeriodId=`
- `GET /api/reports/attendance?programId=&academicPeriodId=&from=&to=`
- `GET /api/reports/assessments?programId=&academicPeriodId=`
- `GET /api/reports/finances/{studentId}?asOf=`
- `GET /api/reports/enrollments.xlsx` (same filters)
- `GET /api/reports/attendance.xlsx` (same filters)
- `GET /api/reports/assessments.xlsx` (same filters)
- `GET /api/reports/finances/{studentId}.xlsx?asOf=`

The XLSX exports use the same MediatR queries as the JSON endpoints.

## Quality gates

Run `dotnet build PaquitoBot.sln` and `dotnet test PaquitoBot.Reports.Tests/PaquitoBot.Reports.Tests.csproj` after applying the package. The isolated test project is deliberately **not** added to the main solution, matching the team's current decision to keep tests outside `PaquitoBot.sln`. Do not merge without review and an integration check against a nonproduction database.
