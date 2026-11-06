# Contribution Guidelines

This document defines the development standards and collaboration workflow for **PaquitoBot API**.

All contributors must follow these guidelines to maintain code quality, consistency, traceability, and a clean Git history.

## 1. Development Workflow

All development tasks should follow this workflow:

**Issue → Branch → Development → Commits → Pull Request → Code Review → Merge**

### Branch Model

| Branch | Purpose |
|---|---|
| `development` | Default and integration branch. All Pull Requests target it. |
| `main` | Stable branch. Only updated from `development` when the team decides to release. |

### General Rules

- Every development task must have an associated GitHub Issue.
- Every Pull Request must reference at least one Issue.
- Direct commits to `main` and `development` are not allowed.
- Each branch should address one Issue or a clearly defined scope.
- All branch names, commit messages, Issue titles, and Pull Request titles must be written in English.
- Code must compile successfully before submitting a Pull Request.
- Never commit passwords, API keys, connection strings containing credentials, or other sensitive information.
- Pull Requests require at least one approval before merging.

---

## 2. Branch Naming Convention

Branches must follow this format:

`<type>/<short-description>`

| Type | Description | Example |
|---|---|---|
| `feature` | New functionality | `feature/student-management` |
| `fix` | Bug fixes | `fix/student-validation` |
| `refactor` | Code improvements without changing behavior | `refactor/repository-layer` |
| `chore` | Maintenance and configuration | `chore/database-setup` |
| `docs` | Documentation changes | `docs/contribution-guidelines` |
| `test` | Test implementation or updates | `test/student-service` |

### Branch Rules

- Use lowercase letters.
- Separate words using hyphens (`-`).
- Avoid spaces, special characters, and unnecessarily long names.
- Create branches from the latest `development`.
- Do not reuse branches after their Pull Requests have been merged.

**Example:**

```bash
git switch development
git pull origin development
git switch -c feature/student-synchronization
```

---

## 3. Commit Message Convention

All commits must follow the **Conventional Commits** specification.

### Format

`<type>(<scope>): <short-description>`

### Commit Types

| Type | Purpose |
|---|---|
| `feat` | Introduce a new feature |
| `fix` | Fix a bug |
| `refactor` | Improve code without changing its behavior |
| `chore` | Maintenance and configuration changes |
| `docs` | Update documentation |
| `test` | Add or modify tests |
| `build` | Modify build configuration or dependencies |
| `perf` | Improve application performance |
| `ci` | Modify CI/CD workflows |

### Commit Scopes

| Scope | Project or Responsibility |
|---|---|
| `api` | API controllers, endpoints and configuration |
| `app` | Application layer, DTOs and use cases |
| `dom` | Domain entities and business rules |
| `inf` | Infrastructure and persistence |
| `db` | Database schema and migrations |
| `auth` | Authentication and authorization |
| `config` | General configuration |

### Commit Examples

```text
feat(api): add student endpoints
feat(app): implement student synchronization use case
feat(dom): add student entity
feat(inf): implement generic repository
feat(db): add initial database schema

fix(api): handle invalid student requests
fix(inf): resolve database connection timeout

refactor(app): simplify student query handler
refactor(inf): improve unit of work implementation

chore(config): configure User Secrets
chore(inf): remove temporary database context

build(api): update Entity Framework Core dependencies
docs: update API setup instructions
test(app): add student service unit tests
ci: add build validation workflow
```

### Commit Rules

- Write commit messages in English.
- Use lowercase for the type, scope, and description.
- Use the imperative mood: `add`, `fix`, `remove`, `update`, `configure`.
- Keep messages short, specific, and meaningful.
- Avoid generic messages such as `changes`, `update`, `fix stuff`, or `final version`.
- Each commit should represent one logical change.
- Breaking changes must be explicitly documented using `!` or a `BREAKING CHANGE:` footer.

---

## 4. GitHub Issue Convention

Every feature, bug fix, refactoring task, or significant configuration change must begin with a GitHub Issue.

Issues are created from the templates in [`.github/ISSUE_TEMPLATE`](.github/ISSUE_TEMPLATE): **Feature request** (also used for refactoring, chore, docs, and test tasks) and **Bug report**.

### Issue Title Format

`[TYPE] Short description`

### Issue Types

| Type | Purpose |
|---|---|
| `FEATURE` | New functionality |
| `BUG` | Unexpected behavior or defect |
| `REFACTOR` | Internal code improvements |
| `CHORE` | Maintenance or configuration |
| `DOCS` | Documentation updates |
| `TEST` | Testing tasks |

The templates prefill `[FEATURE]` or `[BUG]`; replace it with the right type when needed.

### Example Issue

**Title:** `[FEATURE] Implement student synchronization`

```markdown
## Summary
Implement student data synchronization between
Frappe Education and PaquitoBot API.

## Objective
Retrieve and synchronize student information
from the external LMS into the local database.

## Tasks
- [ ] Configure Frappe API communication.
- [ ] Implement student synchronization service.
- [ ] Map external student data to local entities.
- [ ] Handle synchronization errors.

## Acceptance Criteria
- [ ] Student records can be retrieved from Frappe.
- [ ] Student data is stored correctly in PostgreSQL.
- [ ] Existing records are updated without duplication.
- [ ] Synchronization failures are handled appropriately.

## Additional Context
Integration with Frappe Education REST API.
```

### Issue Rules

- Use clear and descriptive titles.
- Each Issue must have a defined objective.
- Acceptance criteria must be measurable and verifiable.
- Avoid combining unrelated requirements.
- Assign the Issue to the responsible developer when possible.
- Apply appropriate labels and priorities.
- Keep the Issue updated when requirements change.

---

## 5. Pull Request Convention

All code changes must be submitted through a Pull Request targeting `development`.

The description is prefilled from [`.github/pull_request_template.md`](.github/pull_request_template.md). Complete every section before requesting a review.

### PR Title Format

`<type>(<scope>): <short-description>`

**Examples:**

```text
feat(api): implement student endpoints
feat(inf): add database scaffolding
fix(app): correct student synchronization logic
refactor(inf): improve repository implementation
chore(config): configure database connection
```

### Linking Pull Requests to Issues

Use GitHub closing keywords to link Pull Requests to their corresponding Issues.

| Keyword | Behavior |
|---|---|
| `Closes #12` | Links and automatically closes Issue #12 when merged into the default branch (`development`) |
| `Fixes #12` | Same behavior, commonly used for bug fixes |
| `Resolves #12` | Same behavior |
| `Refs #12` | References the Issue without automatically closing it |

When a Pull Request only partially addresses an Issue, use `Refs #12` instead.

### Pull Request Rules

- Every PR must reference at least one Issue.
- PR titles and descriptions must be written in English.
- Keep changes focused on the associated Issue.
- Complete the PR template before requesting a review.
- Ensure the solution builds successfully.
- Include evidence when applicable.
- Resolve relevant review comments before merging.
- Do not include unrelated changes.
- Never expose credentials or sensitive data.

---

## 6. Code Review and Merge Rules

### Before Requesting a Review

- [ ] The branch is updated with the latest `development` changes.
- [ ] The solution builds successfully.
- [ ] The code follows project conventions.
- [ ] Relevant tests have been executed.
- [ ] Debugging code and unnecessary files have been removed.
- [ ] No secrets or sensitive information are included.
- [ ] The associated Issue is correctly linked.

### Merge Requirements

- At least one approval from another team member.
- No unresolved blocking review comments.
- No merge conflicts.
- All required automated checks must pass.
- Prefer **Squash and merge** for a clean and readable history.
- Delete the source branch after merging when it is no longer needed.

---

## 7. Database and Security Guidelines

Since PaquitoBot uses **PostgreSQL on Neon** and **Entity Framework Core**, contributors must follow these additional rules:

- Never commit database credentials.
- Store local credentials using .NET User Secrets.
- Do not add sensitive connection strings to `appsettings.json`.
- Coordinate database schema changes with the team.
- Avoid destructive database operations without explicit approval.
- Review generated entities and DbContext changes before committing.
- Document any required migration or database configuration.
- Never modify the shared database schema without informing the team.
- Do not commit local development artifacts such as `bin/` or `obj/`.

---

## 8. Recommended GitHub Labels

| Label | Description |
|---|---|
| `feature` | New functionality |
| `bug` | Bug or unexpected behavior |
| `refactor` | Code improvements |
| `chore` | Maintenance tasks |
| `documentation` | Documentation changes |
| `testing` | Testing tasks |
| `database` | Database-related changes |
| `priority:high` | High-priority task |
| `priority:medium` | Medium-priority task |
| `priority:low` | Low-priority task |
| `blocked` | Work blocked by a dependency |

---

## 9. Complete Workflow Example

**Step 1 — Create an Issue**

`[FEATURE] Implement student synchronization`

**Step 2 — Create a Branch**

```bash
git switch development
git pull origin development
git switch -c feature/student-synchronization
```

**Step 3 — Develop and Commit**

```bash
git add .
git commit -m "feat(app): implement student synchronization"
```

**Step 4 — Push the Branch**

```bash
git push -u origin feature/student-synchronization
```

**Step 5 — Create a Pull Request**

Open the PR against `development`.

PR title:

`feat(app): implement student synchronization`

PR description must include:

```markdown
## Related Issue
Closes #12
```

**Step 6 — Review and Merge**

After approval and successful validation, merge the Pull Request into `development`.

**Step 7 — Close the Issue**

GitHub automatically closes the linked Issue when the PR is merged into the default branch using a supported closing keyword.

---

## 10. Definition of Done

A development task is considered complete when:

- [ ] All acceptance criteria are satisfied.
- [ ] The implementation has been validated.
- [ ] The solution builds without errors.
- [ ] Relevant tests pass.
- [ ] Documentation is updated when necessary.
- [ ] The Pull Request has been reviewed and approved.
- [ ] The PR is merged into `development`.
- [ ] The associated Issue is closed.

Following these conventions ensures consistent collaboration, clear traceability, and maintainable development across the PaquitoBot API project.
