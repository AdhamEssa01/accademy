# Academy Backend ExecPlan (Updated) — after Task 1.3

## Context
Tasks 0.1–1.3 are completed:
- Clean Architecture projects (Api/Application/Domain/Infrastructure/Shared) + tests
- EF Core + Identity (Guid keys), migrations + dev seeding
- ProblemDetails middleware, FluentValidation, pagination helpers
- API versioning + Swagger + health checks
- JWT access/refresh tokens, Google login
- RBAC policies + tenant (AcademyId) scoping with query filters
- Current user context + tenant guard + debug endpoints

## Key product decision
Payments / billing / fees are deferred.
Do NOT implement any payment-related domain models, endpoints, migrations, or UI contracts.

## Global rules for all milestones
- Keep file structure tidy and names expressive:
  - Domain: Entities, Enums, Interfaces
  - Application: Contracts (Requests/Responses), Validators, Services/UseCases, Exceptions
  - Infrastructure: Persistence (DbContext, Configurations, Migrations), Auth providers, Storage providers
  - Api: Controllers, Middleware, Options, Swagger, Extensions
- Delete unused files ONLY if:
  - They are not referenced anywhere (search usage)
  - dotnet test passes afterward
  - You also update docs/tests if they referenced them
- Every academy-scoped entity MUST implement IAcademyScoped and rely on tenant filters (no IgnoreQueryFilters).
- All endpoints must use Policies (Admin/Instructor/Parent/Student/Staff/AnyAuthenticated).
- After each milestone:
  - Run: dotnet test
  - Update the Progress checklist
  - Commit: "milestone X.Y - <short title>"

## Progress
### Foundation updates
- [x] Milestone 1.4 - Switch Dev DB to SQL Server (SSMS-visible) + keep tests isolated
- [x] Milestone 1.5 - Security hardening baseline (rate limit, headers, CORS, secrets)

### Core academy structure
- [x] Milestone 2.1 - Academy & Branch management (Admin)
- [x] Milestone 2.2 - Programs/Courses/Levels CRUD (Admin)
- [x] Milestone 2.3 - Groups & Sessions (Admin + Instructor views)
- [x] Milestone 2.4 - Weekly Timetable (Routine) derived to Sessions (Admin + Instructor view)

### Students & Parents
- [ ] Milestone 3.1 - Students CRUD + Photo Upload (Staff)
- [ ] Milestone 3.2 - Guardians + Parent portal read APIs (Admin + Parent)
- [ ] Milestone 3.3 - Enrollments (Student <-> Group history)

### Attendance
- [ ] Milestone 4.1 - Attendance (bulk per session) + permissions
- [ ] Milestone 4.2 - Attendance queries + basic reporting endpoints

### Learning workflow (no payments)
- [ ] Milestone 4.3 - Homework/Assignments (Staff create, Parent read)
- [ ] Milestone 4.4 - Communication/Announcements + In-app notifications (Admin/Staff -> Parent)

### Quality gate
- [ ] Milestone 4.9 - Codebase hygiene (remove demo endpoints if not needed, dotnet format, delete unused)

---

# Milestone 1.4 - Switch Dev DB to SQL Server (SSMS-visible) + keep tests isolated

## Goal
Make the local dev database show up in SSMS.
- Development runtime DB provider: SQL Server (LocalDB preferred).
- Tests keep using SQLite (or in-memory) for isolation/speed.
- Migrations remain in Academy.Infrastructure.

## Changes
1) Introduce a provider switch in configuration:
- appsettings.Development.json:
  - Database:Provider = "SqlServer"
  - ConnectionStrings:Default = "Server=(localdb)\\MSSQLLocalDB;Database=AcademyDev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
- appsettings.json:
  - Database:Provider = "Sqlite" (optional fallback) or omit

2) Update AddInfrastructure(...) to select provider based on Database:Provider:
- If "SqlServer": UseSqlServer(connString, b => b.MigrationsAssembly(...))
- If "Sqlite": UseSqlite(connString, Consider migrations assembly)
- Default to SqlServer in Development if not specified.

3) Ensure design-time factory supports SQL Server by default (read appsettings.Development.json if present).

4) Update HealthChecks readiness DB check to still use AppDbContext.

5) Docs:
- README: how to connect in SSMS:
  - Server name: (localdb)\\MSSQLLocalDB
  - Database: AcademyDev
- Add command:
  - dotnet ef database update -p src/Academy.Infrastructure -s src/Academy.Api

## Verification
- dotnet test
- dotnet run --project src/Academy.Api
- Confirm in SSMS you can see AcademyDev

---

# Milestone 1.5 - Security hardening baseline (rate limit, headers, CORS, secrets)

## Goal
Harden the API before adding more modules.

## Requirements
1) Secrets hygiene:
- Move Jwt:Key out of appsettings*.json for dev:
  - Use dotnet user-secrets in Academy.Api
  - README shows how to set: dotnet user-secrets set "Jwt:Key" "..."
- Keep a fallback for CI/tests via environment variables.

2) Rate limiting (built-in .NET):
- Add Microsoft.AspNetCore.RateLimiting
- Configure policies:
  - "auth" fixed window: 10 req/min/IP for:
    /api/v1/auth/login
    /api/v1/auth/register
    /api/v1/auth/refresh
    /api/v1/auth/google
  - "general" fixed window: 120 req/min/IP for other endpoints
- Ensure 429 responses are ProblemDetails.

3) Security headers middleware:
- Add simple response headers:
  - X-Content-Type-Options: nosniff
  - X-Frame-Options: DENY
  - Referrer-Policy: no-referrer
  - Permissions-Policy: minimal (optional)
- HTTPS:
  - app.UseHttpsRedirection()
  - HSTS enabled only outside Development

4) CORS allowlist:
- Read AllowedOrigins from config and allow only those.
- Disallow wildcard in Production.

5) Request limits:
- Limit multipart upload size (e.g., 2–5 MB for student photos)
- Validate allowed content types for uploads

## Tests
- Add at least one integration test:
  - auth rate limit returns 429 after exceeding threshold (can be a light test with retries)
  - CORS headers present for allowed origin (optional)

## Verification
- dotnet test

---

# Milestone 2.1 - Academy & Branch management (Admin)
(Keep the same spec as previously planned: AcademiesController + BranchesController, Branch is academy-scoped.)
- Branch: IAcademyScoped, unique (AcademyId, Name)
- Admin-only endpoints
- Migration: AddBranches
- Integration tests for permissions and CRUD

---

# Milestone 2.2 - Programs/Courses/Levels CRUD (Admin)
(Keep the same spec as previously planned.)
- Program/Course/Level are academy-scoped
- Unique indexes
- Migration: AddProgramStructure
- Integration tests include tenant isolation using the existing debug seed second academy pattern

---

# Milestone 2.3 - Groups & Sessions (Admin + Instructor views)
(Keep the same spec as previously planned.)
- Group and Session are academy-scoped
- Instructor "mine" endpoints filtered by InstructorUserId
- Migration: AddGroupsAndSessions
- Tests for permissions and filtering

---

# Milestone 2.4 - Weekly Timetable (Routine) derived to Sessions (Admin + Instructor view)

## Goal
Add a Weekly Routine layer to represent recurring schedule, and optionally generate Sessions.

## Domain/Entities
RoutineSlot : IAcademyScoped
- Id, AcademyId
- GroupId
- DayOfWeek (0..6)
- TimeOnly StartTime
- int DurationMinutes (15..360)
- Guid InstructorUserId (required)
- DateTime CreatedAtUtc
Unique index: (AcademyId, GroupId, DayOfWeek, StartTime)

Optional (nice):
- Endpoint to "Generate sessions for date range" using routine slots.

## API
RoutineController:
- Admin CRUD: /api/v1/routine-slots
- Instructor view: /api/v1/routine-slots/mine

Session generation (Admin):
- POST /api/v1/routine-slots/generate-sessions?from=YYYY-MM-DD&to=YYYY-MM-DD
- Creates Sessions only if not already exist for that group + start time.

## Tests
- Instructor sees only their routine slots.
- Generating sessions creates expected number without duplicates.

## Migration
- AddRoutineSlots

---

# Milestone 3.1 - Students CRUD + Photo Upload (Staff)
(As previously planned; keep upload safe, serve static files, enforce content type & size.)

---

# Milestone 3.2 - Guardians + Parent portal read APIs (Admin + Parent)
(As previously planned; parent sees only linked children.)

---

# Milestone 3.3 - Enrollments (Student <-> Group history)
(As previously planned; record start/end.)

---

# Milestone 4.1 - Attendance (bulk per session) + permissions
(As previously planned; upsert + permission check.)

---

# Milestone 4.2 - Attendance queries + reporting endpoints
(As previously planned; staff filters + parent restricted.)

---

# Milestone 4.3 - Homework/Assignments (Staff create, Parent read)

## Goal
Add homework module without payments.

## Domain/Entities
Assignment : IAcademyScoped
- Id, AcademyId
- GroupId
- Title (required max 200)
- Description? (max 2000)
- DueAtUtc? (optional)
- CreatedByUserId
- CreatedAtUtc

AssignmentAttachment : IAcademyScoped
- Id, AcademyId
- AssignmentId
- FileUrl (max 500)
- FileName (max 255)
- ContentType (max 100)
- CreatedAtUtc

AssignmentTarget : IAcademyScoped
- Id, AcademyId
- AssignmentId
- StudentId (optional)  // null means whole group
- CreatedAtUtc

## API
Staff:
- POST /api/v1/assignments (create for group; optionally target students)
- POST /api/v1/assignments/{id}/attachments (upload)
- GET  /api/v1/assignments?groupId=&from=&to=&page=&pageSize=
Parent:
- GET /api/v1/parent/me/assignments?from=&to=&page=&pageSize=
Rules:
- Parent list must include only assignments for children’s groups OR explicit student targets.

## Migration
- AddAssignments

## Tests
- Staff create assignment, parent sees it only for their child.

---

# Milestone 4.4 - Communication/Announcements + In-app notifications

## Goal
Add announcements and notifications (no SMS/WhatsApp integration yet).

## Domain/Entities
Announcement : IAcademyScoped
- Id, AcademyId
- Title (required max 200)
- Body (required max 5000)
- Audience enum: AllParents, AllStaff, GroupParents, GroupStaff
- GroupId? (when targeting group)
- PublishedAtUtc
- CreatedByUserId
- CreatedAtUtc

Notification already exists:
- Ensure notifications are created for targeted users when an announcement is published.

## API
Staff:
- POST /api/v1/announcements
- GET  /api/v1/announcements?page=&pageSize=
Parent:
- GET /api/v1/parent/me/announcements?page=&pageSize=
Notifications:
- GET /api/v1/notifications
- POST /api/v1/notifications/{id}/read

## Tests
- Publishing an announcement creates notifications for target users.
- Parent sees only parent-targeted announcements.

## Migration
- AddAnnouncements

---

# Milestone 4.9 - Codebase hygiene

## Goal
Keep project reliable and clean.

## Actions
- Remove DemoController endpoints if they are no longer required by tests.
  - If removing: also remove demo validators/tests and update swagger tests accordingly.
- Ensure all file names and namespaces follow the folder structure.
- Delete unused files after reference search.
- Run dotnet format (if configured) or at least ensure no analyzer warnings.

## Verification
- dotnet test

---

# Stop point
After Milestone 4.9 completes, stop.
Next plan will cover: Evaluations, Behavior, Exams/Tests, CMS (home/landing dashboard).
