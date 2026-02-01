# Academy Backend ExecPlan — Phase 2 (After Master Plan Completion)

## Assumed current state (before starting this plan)
- Master plan milestones are completed (DB -> SQL Server, security baseline, homework/announcements/routine, hygiene).
- The system uses SQL Server locally and is runnable end-to-end.
- Tenant scoping (AcademyId) and RBAC policies are already in place.

## Hard requirements for this phase
- SQL Server ONLY (no SQLite in codebase).
- ConnectionString MUST use: Server=.
- Remove ALL SQLite references (packages, config, UseSqlite, docs, tests).
- Continue using Clean Architecture and tenant query filters for IAcademyScoped entities.

## Global rules
- No payment/billing/fees modules.
- Keep controllers thin; business logic in Application.
- All academy-scoped entities implement IAcademyScoped and rely on query filters (no IgnoreQueryFilters).
- Authorization required on all endpoints except explicitly public CMS read endpoints.
- After each milestone:
  - dotnet test
  - Update Progress checklist
  - Commit: "milestone X.Y - <short title>"

## Progress
### SQL Server enforcement
- [x] Milestone 2.0 - SQL Server only (Server=.) + purge SQLite completely

### Learning modules
- [x] Milestone 5.1 - Evaluation templates (Rubrics) CRUD (Admin)
- [x] Milestone 5.2 - Student evaluations CRUD (Instructor/Staff) + Parent read

- [x] Milestone 6.1 - Behavior events (points) CRUD (Staff) + Parent read
- [x] Milestone 6.2 - Behavior summaries & risk flags (Admin/Staff dashboards)

- [x] Milestone 7.1 - Question bank (CRUD) (Staff)
- [x] Milestone 7.2 - Exam builder (create exams + attach questions) (Staff)
- [ ] Milestone 7.3 - Exam assignment (group/student, open/close, attempts) (Staff)
- [ ] Milestone 7.4 - Attempts (start/save/submit) (Student optional) + Parent read results
- [ ] Milestone 7.5 - Auto grading + scoring (MCQ/TF/Fill) (System)
- [ ] Milestone 7.6 - Manual grading (essay/file) (Instructor/Staff)
- [ ] Milestone 7.7 - Exam analytics endpoints (Admin/Staff)

### CMS / Website content
- [ ] Milestone 8.1 - CMS Pages & Sections (Admin edit/publish) + Public read endpoints
- [ ] Milestone 8.2 - Achievements CRUD (Admin) + Public read endpoints

### Dashboards
- [ ] Milestone 10.1 - Admin dashboard endpoints (KPIs: attendance, evals, behavior, exams)
- [ ] Milestone 10.2 - Instructor dashboard endpoints (today sessions, pending grading, recent evals)
- [ ] Milestone 10.3 - Parent dashboard endpoints (children summary, attendance, homework, announcements, results)

### Final quality gate
- [ ] Milestone 99.0 - Reliability & cleanup (remove dead code/files, tighten security, docs)

---

# Milestone 2.0 - SQL Server only (Server=.) + purge SQLite completely

## Goal
Make the codebase strictly SQL Server and remove SQLite completely.

## Required configuration
In Academy.Api appsettings.Development.json:
- ConnectionStrings:Default =
  "Server=.;Database=AcademyDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

No provider switch. Always UseSqlServer.

## Code changes
1) Infrastructure DI:
- Remove any Database:Provider logic / UseSqlite branches.
- Always: options.UseSqlServer(connString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

2) Design-time factory:
- Ensure it creates AppDbContext with SQL Server using the same connection string pattern (Server=.).
- Must work with `dotnet ef migrations add` and `dotnet ef database update`.

3) Remove ALL SQLite packages and references:
- Delete from csproj:
  - Microsoft.EntityFrameworkCore.Sqlite
  - Any SQLite-specific test helpers
- Remove any Sqlite connection strings in docs/config.

4) Integration tests isolation WITHOUT SQLite:
- Update Academy.Api.Tests to use SQL Server with unique DB per test run:
  - DB name pattern: AcademyTest_<GUID>
  - On test startup:
    - Create DB (or let EF create via Migrate)
  - On test teardown:
    - Drop DB safely
- Ensure tests are reliable and parallel-safe:
  - Option: disable parallelization for integration tests OR ensure per-test DB name uniqueness.

## Verification
- dotnet test
- dotnet run --project src/Academy.Api
- Confirm in SSMS (Server=.) that AcademyDev exists and has tables.

---

# Milestone 5.1 - Evaluation templates (Rubrics) CRUD (Admin)

## Goal
Admin manages evaluation templates per Program/Course/Level, with rubric criteria and weights.

## Domain entities (all IAcademyScoped)
1) EvaluationTemplate
- Id, AcademyId
- ProgramId? (optional)
- CourseId? (optional)
- LevelId? (optional)
- Name (required max 200)
- Description? (max 800)
- CreatedAtUtc

2) RubricCriterion
- Id, AcademyId
- TemplateId
- Name (required max 150)
- MaxScore (1..100)
- Weight (0.0..10.0)
- SortOrder (>=0)
- CreatedAtUtc

Constraints:
- Unique: (AcademyId, TemplateId, Name)

## Infrastructure
- DbSets + configuration + migration: AddEvaluationsTemplates

## Application
- DTOs + Validators:
  - Create/Update EvaluationTemplate
  - Create/Update/Delete RubricCriterion
- Services:
  - IEvaluationTemplateService CRUD + criteria CRUD
- All list endpoints paged.

## API (Admin)
- /api/v1/evaluation-templates (CRUD)
- /api/v1/evaluation-templates/{id}/criteria (CRUD)

## Tests
- Admin can create template + criteria
- Parent cannot access (403)
- Tenant isolation test

---

# Milestone 5.2 - Student evaluations (Instructor/Staff create) + Parent read

## Goal
Instructor/staff can evaluate a student (session-based or periodic).
Parent can read evaluations for their children.

## Domain entities (IAcademyScoped)
1) Evaluation
- Id, AcademyId
- StudentId
- TemplateId
- SessionId? (optional)
- EvaluatedByUserId
- Notes? (max 1000)
- TotalScore (>=0)
- CreatedAtUtc

2) EvaluationItem
- Id, AcademyId
- EvaluationId
- CriterionId
- Score (0..MaxScore)
- Comment? (max 500)

Constraints:
- Unique: (AcademyId, EvaluationId, CriterionId)

## Application
- Services:
  - IEvaluationService:
    - CreateAsync(studentId, templateId, items, notes, sessionId?)
    - ListForStudentAsync(studentId, paged)
    - ParentListMyChildrenAsync(paged, from?, to?)
- Rules:
  - Staff can evaluate any student in tenant.
  - Instructor can evaluate students only in groups they teach OR session instructor match (choose strict: session match OR enrollment in their group).
  - Parent sees only children.

## API
Staff:
- POST /api/v1/evaluations
- GET  /api/v1/students/{id}/evaluations?page=&pageSize=
Parent:
- GET /api/v1/parent/me/evaluations?from=&to=&page=&pageSize=

## Tests
- Instructor creates evaluation for own student works; other instructor gets 403.
- Parent sees only child evaluations.

---

# Milestone 6.1 - Behavior events CRUD (Staff) + Parent read

## Domain entities (IAcademyScoped)
BehaviorEvent
- Id, AcademyId
- StudentId
- SessionId? (optional)
- Type enum: Positive, Negative
- Points (range -20..20; negative allowed)
- Reason (required max 200)
- Note? (max 500)
- CreatedByUserId
- CreatedAtUtc

## Infrastructure
- DbSet + migration: AddBehaviorEvents

## Application
- IBehaviorService:
  - CreateAsync(...)
  - ListForStudentAsync(studentId, from?, to?, paged)
  - ParentListMyChildrenAsync(from?, to?, paged)

## API
Staff:
- POST /api/v1/behavior-events
- GET  /api/v1/students/{id}/behavior-events?from=&to=&page=&pageSize=
Parent:
- GET /api/v1/parent/me/behavior-events?from=&to=&page=&pageSize=

## Tests
- Staff create + list works
- Parent sees only children

---

# Milestone 6.2 - Behavior summaries & risk flags (Admin/Staff dashboards)

## Goal
Provide summary endpoints:
- Current points (weekly/monthly)
- Risk flags: high absence + negative behavior + low evaluations (simple thresholds)

## Application
- IStudentRiskService:
  - GetRiskListAsync(from,to, paged)
- Keep thresholds in config (appsettings) or constants.

## API (Staff)
- GET /api/v1/students/risk?from=&to=&page=&pageSize=

## Tests
- Returns paged shape and filters by tenant.

---

# Milestone 7.1 - Question bank (CRUD) (Staff)

## Domain entities (IAcademyScoped)
Question
- Id, AcademyId
- ProgramId?, CourseId?, LevelId?
- Type enum: MCQ, TrueFalse, FillBlank, Essay, FileUpload
- Text (required max 4000)
- Difficulty enum: Easy, Medium, Hard
- Tags (string? max 500) OR separate table QuestionTag (optional)
- CreatedByUserId
- CreatedAtUtc

QuestionOption (for MCQ/TF)
- Id, AcademyId
- QuestionId
- Text (required max 1000)
- IsCorrect (bool)
- SortOrder (>=0)

## Infrastructure
- DbSets + migration: AddQuestionBank

## API (Staff)
- /api/v1/questions (CRUD + paging + filters)

## Tests
- Staff CRUD works; parent forbidden.

---

# Milestone 7.2 - Exam builder (create exams + attach questions) (Staff)

## Domain entities (IAcademyScoped)
Exam
- Id, AcademyId
- Title (required max 200)
- Type enum: Quiz, Exam, Placement
- DurationMinutes (1..240)
- ShuffleQuestions (bool)
- ShuffleOptions (bool)
- ShowResultsAfterSubmit (bool)
- CreatedByUserId
- CreatedAtUtc

ExamQuestion
- Id, AcademyId
- ExamId
- QuestionId
- Points (0..100)
- SortOrder (>=0)

Migration: AddExams

## API (Staff)
- /api/v1/exams (CRUD)
- /api/v1/exams/{id}/questions (add/remove/reorder)

## Tests
- Build exam with questions.

---

# Milestone 7.3 - Exam assignment (group/student, open/close, attempts) (Staff)

## Domain entities (IAcademyScoped)
ExamAssignment
- Id, AcademyId
- ExamId
- GroupId? (optional)
- StudentId? (optional)
- OpenAtUtc
- CloseAtUtc
- AttemptsAllowed (1..5)
- CreatedAtUtc

Rules:
- Either GroupId or StudentId must be set.

Migration: AddExamAssignments

## API (Staff)
- POST /api/v1/exams/{id}/assignments
- GET  /api/v1/exams/{id}/assignments

## Tests
- Create assignment for group and for student.

---

# Milestone 7.4 - Attempts (start/save/submit) + Parent read results

## Domain entities (IAcademyScoped)
ExamAttempt
- Id, AcademyId
- AssignmentId
- StudentId
- StartedAtUtc
- SubmittedAtUtc?
- Status enum: InProgress, Submitted, Graded
- TotalScore (>=0)
- CreatedAtUtc

AttemptAnswer
- Id, AcademyId
- AttemptId
- QuestionId
- AnswerJson (required)
- IsCorrect?
- Score?
- Feedback? (max 500)

Migration: AddExamAttempts

## API
Student (optional now; can be AnyAuthenticated but must be Student role if you created Student users):
- POST /api/v1/assignments/{id}/attempts/start
- PUT  /api/v1/attempts/{id}/answers  (autosave)
- POST /api/v1/attempts/{id}/submit

Parent:
- GET /api/v1/parent/me/exam-results?page=&pageSize=

Staff:
- GET /api/v1/exams/{id}/results?page=&pageSize=

## Tests
- Start->submit works with basic happy path.
- Parent sees only children results.

---

# Milestone 7.5 - Auto grading + scoring (MCQ/TF/Fill)

## Goal
Grade submitted attempts automatically for supported question types.

## Application
- IExamGradingService:
  - GradeAttemptAsync(attemptId) -> sets per-answer score and total score, status = Graded if no manual items.
Rules:
- Essay/FileUpload remain pending manual grading.

## Tests
- Auto-graded attempt gets correct score.

---

# Milestone 7.6 - Manual grading (essay/file) (Instructor/Staff)

## API (Staff)
- POST /api/v1/attempt-answers/{id}/grade  { score, feedback }
- Permission: Instructor can grade attempts for assignments in their group, or Staff always.

## Tests
- Instructor can grade own group; not others.

---

# Milestone 7.7 - Exam analytics endpoints (Admin/Staff)

## Endpoints
- GET /api/v1/exams/{id}/stats
  - average score, count attempts, distribution buckets (simple), most missed questions (top 5)

## Tests
- Stats returns expected shape.

---

# Milestone 8.1 - CMS Pages & Sections (Admin edit/publish) + Public read

 "Public website content" for Landing/Home and basic pages.

## Domain (IAcademyScoped)
CmsPage
- Id, AcademyId
- Slug (required max 50) e.g. "home", "landing", "about"
- Title (max 200)
- PublishedAtUtc?
- CreatedAtUtc

CmsSection
- Id, AcademyId
- PageId
- Type (required max 50) e.g. "hero", "stats", "programs"
- JsonContent (required)
- SortOrder (>=0)
- IsVisible (bool)
- CreatedAtUtc

Migration: AddCms

## API
Admin:
- GET/PUT /api/v1/cms/pages/{slug}
- PUT /api/v1/cms/pages/{slug}/sections (bulk reorder/update)
Public (AllowAnonymous):
- GET /api/v1/public/cms/pages/{slug}

Rules:
- Public endpoints must return only Published pages/sections.

## Tests
- Admin edits; public reads published only.

---

# Milestone 8.2 - Achievements CRUD + Public read

## Domain (IAcademyScoped)
Achievement
- Id, AcademyId
- Title (required max 200)
- Description? (max 2000)
- DateUtc (required)
- MediaUrl? (max 500)
- Tags? (max 200)
- CreatedAtUtc

Migration: AddAchievements

## API
Admin:
- /api/v1/achievements (CRUD)
Public:
- GET /api/v1/public/achievements?page=&pageSize=

## Tests
- Public lists only tenant’s published (if you introduce published flag; otherwise all).

---

# Milestones 10.1–10.3 Dashboards

## Goal
Expose role dashboards (read-only aggregated endpoints).

Admin dashboard:
- attendance today summary
- risky students list (from 6.2)
- pending manual grading count
- last 7 days exam attempts stats

Instructor dashboard:
- today sessions mine
- pending grading
- recent attendance submission status

Parent dashboard:
- children summary (attendance rate, last eval score, behavior points)
- upcoming assignments (due soon)
- new announcements count
- recent exam results

## API
- GET /api/v1/dashboards/admin
- GET /api/v1/dashboards/instructor
- GET /api/v1/dashboards/parent

## Tests
- Basic shape + correct authorization.

---

# Milestone 99.0 - Reliability & cleanup

## Goal
Keep repo clean and secure.

Actions:
- Remove any demo/debug endpoints not needed (or hide from Swagger).
- Ensure no SQLite strings/packages remain (final scan).
- Ensure SQL Server connection is Server=. for dev.
- Ensure warnings as errors still pass.
- Ensure dotnet test passes.

Stop after completion.
