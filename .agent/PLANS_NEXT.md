# Academy Backend Phase 2 ExecPlan (Focus on product, minimal tests)

## Hard requirements
- SQL Server ONLY (remove SQLite entirely).
- Dev ConnectionString MUST use: Server=.;Database=AcademyDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
- No payments/billing/fees modules in this phase.
- Keep the codebase tidy: expressive file names, delete unused files safely.

## Testing policy (IMPORTANT)
We will keep tests MINIMAL:
- No more than 1–2 integration tests per milestone (smoke tests).
- Prefer integration “happy path” tests over many unit tests.
- Only add a unit test when it prevents a known regression (e.g., a critical validator).
- Target: tests code should not exceed ~15–20% of the non-test code added in a milestone.

## Global rules
- Controllers thin; logic in Application services/use-cases.
- All academy-scoped entities implement IAcademyScoped and rely on tenant filters (no IgnoreQueryFilters).
- Authorization via existing Policies.
- After each milestone:
  - dotnet test
  - update Progress
  - commit "milestone X.Y - <short title>"

## Progress
### Platform stabilization
- [ ] Milestone 2.0 - SQL Server only (Server=.) + purge SQLite + single test DB

### Learning modules (core)
- [ ] Milestone 5.1 - Evaluation templates (Rubrics) CRUD (Admin)
- [ ] Milestone 5.2 - Student evaluations (Staff/Instructor create) + Parent read

- [ ] Milestone 6.1 - Behavior events (points) CRUD (Staff) + Parent read
- [ ] Milestone 6.2 - Risk list endpoint (absence+behavior+evaluations) (Staff)

- [ ] Milestone 7.1 - Question bank CRUD (Staff)
- [ ] Milestone 7.2 - Exam builder (create exams + attach questions) (Staff)
- [ ] Milestone 7.3 - Exam assignments (group/student, window, attempts) (Staff)
- [ ] Milestone 7.4 - Attempts (start/save/submit) + results (Parent read, Staff list)
- [ ] Milestone 7.5 - Auto grading for MCQ/TF/Fill
- [ ] Milestone 7.6 - Manual grading for Essay/FileUpload
- [ ] Milestone 7.7 - Exam analytics (basic stats) (Staff)

### CMS / Website content
- [ ] Milestone 8.1 - CMS Pages & Sections (Admin edit/publish) + Public read
- [ ] Milestone 8.2 - Achievements CRUD + Public read

### Dashboards
- [ ] Milestone 10.1 - Admin dashboard endpoints
- [ ] Milestone 10.2 - Instructor dashboard endpoints
- [ ] Milestone 10.3 - Parent dashboard endpoints

### Final reliability pass
- [ ] Milestone 99.0 - Cleanup + docs + security review

---

# Milestone 2.0 - SQL Server only (Server=.) + purge SQLite + single test DB

## Goal
Use SQL Server only, remove SQLite completely, and stop creating many test databases.

## Required changes
1) Infrastructure DI:
- Always UseSqlServer with migrations assembly.
- Remove any UseSqlite/provider switch.

2) Config:
- appsettings.Development.json -> AcademyDev connection string (Server=.)
- docs updated accordingly.

3) Remove SQLite:
- Remove Microsoft.EntityFrameworkCore.Sqlite and all Sqlite references.

4) Tests minimal but stable:
- Use ONE test DB: AcademyTest
- Disable parallelization for integration tests
- On test suite start: EnsureDeleted + Migrate
- No per-test GUID database creation.

## Tests (minimal)
- 1 smoke integration test: start API + call /health/ready returns 200.

---

# Milestone 5.1 - Evaluation templates (Rubrics) CRUD (Admin)

## Implementation
- Add EvaluationTemplate + RubricCriterion (academy-scoped)
- CRUD endpoints (Admin)
- Migration: AddEvaluationTemplates

## Tests (minimal)
- 1 integration test: Admin creates template + criteria -> 200 and can list.

---

# Milestone 5.2 - Student evaluations + Parent read

## Implementation
- Add Evaluation + EvaluationItem (academy-scoped)
- Staff/Instructor create evaluation
- Parent read for children
- Migration: AddEvaluations

## Tests (minimal)
- 1 integration test: Parent sees only child evaluations (happy path).

---

# Milestone 6.1 - Behavior events + Parent read

## Implementation
- BehaviorEvent entity + CRUD create/list
- Parent read for children
- Migration: AddBehaviorEvents

## Tests (minimal)
- 1 integration test: Parent sees only own child behavior events.

---

# Milestone 6.2 - Risk list endpoint

## Implementation
- /api/v1/students/risk?from=&to=&page=&pageSize=
- Simple scoring from:
  - attendance rate (existing)
  - behavior points
  - latest evaluation score
- thresholds configurable in appsettings

## Tests (minimal)
- 1 integration test: returns paged response shape for staff.

---

# Milestone 7.1–7.7 Exams
(Implement as previously outlined; keep tests to 1 per 2 milestones)
- Only add unit tests for grading logic if needed.

---

# Milestone 8.1–8.2 CMS
- Public endpoints are AllowAnonymous but must only return Published content.

## Tests (minimal)
- 1 integration test: public CMS returns only published.

---

# Milestone 10 dashboards
- Provide aggregated read-only endpoints.

## Tests (minimal)
- 1 integration test: correct authorization (401/403/200) for at least one dashboard.

---

# Milestone 99.0 - Cleanup + docs + security review

## Actions
- Ensure no SQLite remains (final search).
- Ensure Server=. connection string documented.
- Ensure secrets are not in appsettings (Jwt key via user-secrets/env).
- Remove dead code/files safely.
- dotnet test

Stop after completion.
