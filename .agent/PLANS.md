# Academy Backend ExecPlan (starting after Task 1.3)

## Context
Tasks 0.1–1.3 are completed:
- Clean Architecture projects (Api/Application/Domain/Infrastructure/Shared) + tests
- EF Core + Identity (Guid keys), SQLite dev DB, migrations, dev seeding
- ProblemDetails middleware, FluentValidation pipeline, pagination helpers
- API versioning + Swagger + health checks
- JWT access/refresh tokens, Google login
- RBAC policies + tenant (AcademyId) scoping mechanism with query filters
- Current user context + tenant guard + debug endpoints (Dev-gated)

## Global rules for all milestones
- Do not implement modules that are not explicitly included in the milestone.
- Keep controllers thin; put business logic in Application services/use-cases.
- All academy-scoped entities MUST implement IAcademyScoped and be protected by tenant query filters.
- Authorization must use existing Policies (Admin/Instructor/Parent/Student/Staff/AnyAuthenticated).
- Every milestone must end with:
  - dotnet test
  - Progress update (checklist)
  - Git commit: "milestone X.Y - <short title>"

## Progress
- [x] Milestone 2.1 - Academy & Branch management (Admin)
- [x] Milestone 2.2 - Programs/Courses/Levels CRUD (Admin)
- [ ] Milestone 2.3 - Groups & Sessions (Admin + Instructor views)
- [ ] Milestone 3.1 - Students CRUD + Photo Upload (Admin/Staff)
- [ ] Milestone 3.2 - Guardians + Parent portal read APIs (Admin + Parent)
- [ ] Milestone 3.3 - Enrollments (Student <-> Group history)
- [ ] Milestone 4.1 - Attendance (bulk per session) + permissions
- [ ] Milestone 4.2 - Attendance queries + basic reporting endpoints

---

# Milestone 2.1 - Academy & Branch management (Admin)

## Goal
Add minimal Academy and Branch management APIs:
- Admin can view/update their own Academy details.
- Admin can CRUD Branches (optional multi-branch support).

## Domain/Entities (Academy.Domain)
1) Branch : IAcademyScoped
- Guid Id
- Guid AcademyId
- string Name (required, max 200)
- string? Address (max 400)
- DateTime CreatedAtUtc

Academy entity already exists; do NOT make it academy-scoped.

## Infrastructure (Academy.Infrastructure)
- Add DbSet<Branch>
- Configure constraints and indexes:
  - Required Name, max lengths
  - Index on (AcademyId, Name) unique
- Create migration:
  - Name: AddBranches
  - Keep migrations under Infrastructure migrations folder

## Application (Academy.Application)
Create DTOs + validators:
- Academy/UpdateAcademyRequest: Name (2..200)
- Branches:
  - CreateBranchRequest: Name (2..200), Address (0..400)
  - UpdateBranchRequest: Name (2..200), Address (0..400)
Use FluentValidation.

Add services/use-cases:
- IAcademyService:
  - Task<AcademyDto> GetMyAcademyAsync(ct)
  - Task<AcademyDto> UpdateMyAcademyAsync(UpdateAcademyRequest req, ct)
- IBranchService:
  - Task<PagedResponse<BranchDto>> ListAsync(PagedRequest, ct)
  - Task<BranchDto> GetAsync(Guid id, ct)
  - Task<BranchDto> CreateAsync(CreateBranchRequest req, ct)
  - Task<BranchDto> UpdateAsync(Guid id, UpdateBranchRequest req, ct)
  - Task DeleteAsync(Guid id, ct)

Rules:
- Must use tenant scope from ICurrentUserContext/ITenantGuard.
- Branch queries must rely on tenant filter (no IgnoreQueryFilters).

## API (Academy.Api)
Add controllers (versioned):
- AcademiesController:
  - [Authorize(Policy=Policies.Admin)]
  - GET  /api/v1/academies/me
  - PUT  /api/v1/academies/me
- BranchesController:
  - [Authorize(Policy=Policies.Admin)]
  - GET  /api/v1/branches?page=&pageSize=
  - GET  /api/v1/branches/{id}
  - POST /api/v1/branches
  - PUT  /api/v1/branches/{id}
  - DELETE /api/v1/branches/{id}

## Tests
Add integration tests in Academy.Api.Tests:
1) Admin can create and list branches.
2) Parent cannot create branch (403).
Ensure DB is isolated per test run.

## Verification
- dotnet build
- dotnet test

## Output
- Summarize changes, list files, include migration command used.

---

# Milestone 2.2 - Programs/Courses/Levels CRUD (Admin)

## Goal
Implement program structure for multi-discipline academy:
- Program -> Course -> Level
All scoped by AcademyId.

## Domain/Entities (Academy.Domain)
All must implement IAcademyScoped:
1) Program
- Id, AcademyId, Name (required max 150), Description? (max 800), CreatedAtUtc
2) Course
- Id, AcademyId, ProgramId, Name (required max 150), Description? (max 800), CreatedAtUtc
3) Level
- Id, AcademyId, CourseId, Name (required max 150), SortOrder (int), CreatedAtUtc

Relationships:
- Program 1..* Courses
- Course 1..* Levels

## Infrastructure
- Add DbSets and Fluent API configuration:
  - Unique index (AcademyId, Name) on Program
  - Unique index (AcademyId, ProgramId, Name) on Course
  - Unique index (AcademyId, CourseId, Name) on Level
  - FK constraints
- Migration name: AddProgramStructure

## Application
DTOs + validators:
- Create/Update Program/Course/Level requests (name length rules, sortOrder >= 0)
Services:
- IProgramCatalogService with CRUD for all three resources
- List endpoints must be paged (use PagedRequest/PagedResponse)
Filtering:
- Courses list can filter by programId
- Levels list can filter by courseId

## API
Controllers (Admin only):
- ProgramsController: /api/v1/programs
- CoursesController:  /api/v1/courses?programId=
- LevelsController:   /api/v1/levels?courseId=
CRUD endpoints for each.

## Tests
Integration tests:
1) Admin creates Program->Course->Level successfully.
2) Tenant filter works: create a second academy via existing debug endpoint (Dev) and ensure Admin A cannot see Admin B programs (404 or empty list).

## Verification
- dotnet test

---

# Milestone 2.3 - Groups & Sessions (Admin + Instructor views)

## Goal
Create teaching groups and sessions:
- Admin can CRUD groups and sessions.
- Instructor can list their own assigned groups/sessions.

## Domain/Entities
All academy-scoped:
1) Group
- Id, AcademyId
- ProgramId, CourseId, LevelId (required)
- string Name (required max 150)
- Guid InstructorUserId (nullable at creation)
- DateTime CreatedAtUtc

2) Session
- Id, AcademyId
- GroupId (required)
- Guid InstructorUserId (required)  // snapshot at time of session
- DateTime StartsAtUtc (required)
- int DurationMinutes (required, 15..360)
- string? Notes (max 800)
- DateTime CreatedAtUtc

Rules:
- If Group has no instructor, Admin must assign before creating sessions, OR Session creation must accept instructor id explicitly.
- Instructor can only view sessions where Session.InstructorUserId == currentUserId.

## Infrastructure
- DbSets + constraints + indexes
- Migration: AddGroupsAndSessions

## Application
DTOs + validators:
- Create/Update Group
- Assign instructor request
- Create/Update Session
Services:
- IGroupService (CRUD + assign instructor + instructor-specific list)
- ISessionService (CRUD + list by group/date range + instructor list)

## API
Controllers:
- GroupsController:
  - Admin endpoints: CRUD + assign instructor
  - Instructor endpoints: GET /api/v1/groups/mine
- SessionsController:
  - Admin endpoints: CRUD + list
  - Instructor endpoints: GET /api/v1/sessions/mine?from=&to=

Authorization:
- Admin endpoints: Policies.Admin
- Instructor endpoints: Policies.Instructor (and filter by InstructorUserId)

## Tests
Integration tests:
1) Instructor cannot access admin-only group create (403).
2) Instructor /groups/mine returns only their groups.
3) Creating session requires valid group and respects tenant scoping.

## Verification
- dotnet test

---

# Milestone 3.1 - Students CRUD + Photo Upload (Admin/Staff)

## Goal
Add Students module:
- Admin/Staff can CRUD students.
- Upload student photo (multipart) and store URL.
- Use local disk storage abstraction.

## Domain/Entities
Student : IAcademyScoped
- Id, AcademyId
- FullName (required max 200)
- DateOnly? DateOfBirth (optional)
- string? PhotoUrl (max 500)
- string? Notes (max 800)
- DateTime CreatedAtUtc

## Infrastructure
- DbSet<Student>, config + migration: AddStudents
- Create storage abstraction in Shared or Application:
  - IMediaStorage
    - Task<string> SaveAsync(Stream content, string contentType, string fileName, string folder, ct)
- Implement LocalMediaStorage in Academy.Api (or Infrastructure) writing to:
  - /src/Academy.Api/wwwroot/uploads/students/
- Expose static files in Api:
  - app.UseStaticFiles()
- PhotoUrl should be a relative URL like: /uploads/students/<file>

## Application
DTOs + validators:
- CreateStudentRequest, UpdateStudentRequest
Services:
- IStudentService CRUD
- IStudentPhotoService:
  - UploadAsync(studentId, IFormFile file, ct)
Validation:
- File type: allow jpg/png/webp
- Max size: 2MB

## API
StudentsController (Policies.Staff):
- GET /api/v1/students (paged)
- GET /api/v1/students/{id}
- POST /api/v1/students
- PUT /api/v1/students/{id}
- DELETE /api/v1/students/{id}
- POST /api/v1/students/{id}/photo (multipart/form-data)

## Tests
Integration tests:
1) Admin can create student.
2) Parent cannot list students (403).
(Uploading file test optional if complicated; keep minimal.)

## Verification
- dotnet test

---

# Milestone 3.2 - Guardians + Parent portal read APIs (Admin + Parent)

## Goal
Guardians (parents) and linking:
- Admin can CRUD guardians and link them to students.
- Parent can see ONLY their linked students (read-only endpoints).

## Domain/Entities
Guardian : IAcademyScoped
- Id, AcademyId
- string FullName (required max 200)
- string? Phone (max 30)
- string? Email (max 254)
- Guid? UserId  // Identity user id (optional linkage)
- DateTime CreatedAtUtc

StudentGuardian : IAcademyScoped
- Id, AcademyId
- StudentId
- GuardianId
- string Relation (max 50) // Father/Mother/etc
- DateTime CreatedAtUtc
Unique index: (AcademyId, StudentId, GuardianId)

## Infrastructure
- DbSets + config + migration: AddGuardians
- Ensure tenant filters apply.

## Application
DTOs + validators:
- Create/Update Guardian
- LinkGuardianToStudentRequest (relation)
- LinkGuardianToUserRequest (userId)
Services:
- IGuardianService:
  - CRUD guardians
  - Link guardian <-> student
  - Link guardian <-> userId
- IParentPortalService:
  - Task<IReadOnlyList<StudentDto>> GetMyChildrenAsync(ct)
Rules:
- Parent portal uses current userId -> Guardian.UserId -> StudentGuardian -> Student
- Must never expose other students.

## API
GuardiansController (Admin):
- CRUD: /api/v1/guardians
- POST /api/v1/guardians/{guardianId}/link-user
- POST /api/v1/students/{studentId}/guardians/{guardianId}  (link student<->guardian)

ParentPortalController (Parent):
- GET /api/v1/parent/me/children

## Tests
Integration tests:
1) Admin creates guardian + student + links them + links guardian to a parent user (created via /auth/register).
2) Parent calls /parent/me/children and sees exactly that student.
3) Another parent sees empty list.

## Verification
- dotnet test

---

# Milestone 3.3 - Enrollments (Student <-> Group history)

## Goal
Track student membership in groups over time.

## Domain/Entities
Enrollment : IAcademyScoped
- Id, AcademyId
- StudentId
- GroupId
- DateOnly StartDate (required)
- DateOnly? EndDate
- DateTime CreatedAtUtc
Rules:
- Only one active enrollment per student per group at a time.
- When moving student between groups, close previous enrollment (EndDate).

## Infrastructure
- DbSet<Enrollment>, config + migration: AddEnrollments
- Indexes:
  - (AcademyId, StudentId, GroupId, StartDate)
  - Query-friendly indexes on StudentId and GroupId

## Application
DTOs + validators:
- CreateEnrollmentRequest: StudentId, GroupId, StartDate
- EndEnrollmentRequest: EndDate
Services:
- IEnrollmentService:
  - EnrollAsync(req)
  - EndAsync(enrollmentId, endDate)
  - ListByStudentAsync(studentId)
  - ListByGroupAsync(groupId)

## API (Staff)
- POST /api/v1/enrollments
- POST /api/v1/enrollments/{id}/end
- GET /api/v1/students/{id}/enrollments
- GET /api/v1/groups/{id}/enrollments

## Tests
Integration tests:
1) Enroll student to group works.
2) Ending enrollment works.
3) Tenant filter prevents cross-academy access.

## Verification
- dotnet test

---

# Milestone 4.1 - Attendance (bulk per session) + permissions

## Goal
Allow taking attendance per session in bulk.
- Admin or the assigned instructor can take attendance for a session.

## Domain/Entities
AttendanceRecord : IAcademyScoped
- Id, AcademyId
- SessionId
- StudentId
- AttendanceStatus Status (enum: Present, Absent, Late, Excused)
- string? Reason (max 200)
- string? Note (max 500)
- Guid MarkedByUserId
- DateTime MarkedAtUtc
Unique index: (AcademyId, SessionId, StudentId)

## Infrastructure
- DbSet + config + migration: AddAttendance

## Application
DTOs + validators:
- AttendanceItemRequest: StudentId, Status, Reason?, Note?
- SubmitAttendanceRequest: List<AttendanceItemRequest> Items (min 1)
Service:
- IAttendanceService:
  - SubmitForSessionAsync(sessionId, request, ct)
  - Permissions:
    - If Admin -> allowed
    - If Instructor -> allowed only if session.InstructorUserId == current userId
  - Upsert behavior:
    - If record exists -> update
    - Else -> create

## API
SessionsAttendanceController (Staff):
- POST /api/v1/sessions/{sessionId}/attendance
- GET  /api/v1/sessions/{sessionId}/attendance

## Tests
Integration tests:
1) Instructor can submit attendance only for own session.
2) Instructor for another session gets 403.
3) Admin can submit.

## Verification
- dotnet test

---

# Milestone 4.2 - Attendance queries + basic reporting endpoints

## Goal
Provide standard queries (paged + filters) for attendance:
- Admin/Staff can query by group/date/status.
- Parent can query attendance for their children only (read-only).

## Application
Add query endpoints/services:
- IAttendanceQueryService:
  - ListAsync(filters: groupId?, studentId?, from?, to?, status?, paged)
  - ParentListForMyChildrenAsync(from?, to?, paged)
Filtering rules:
- Use tenant filters
- Parent endpoint must restrict to children derived from guardian link (same logic as parent portal)

## API
AttendanceController:
- GET /api/v1/attendance?groupId=&studentId=&from=&to=&status=&page=&pageSize=
  - [Authorize(Policy=Policies.Staff)]
ParentAttendanceController:
- GET /api/v1/parent/me/attendance?from=&to=&page=&pageSize=
  - [Authorize(Policy=Policies.Parent)]

## Tests
Integration tests:
1) Staff attendance list returns expected shape.
2) Parent attendance endpoint returns only their child's records.

## Verification
- dotnet test

---

# Stop point
After Milestone 4.2 completes, stop. Next plan file update will cover:
- Evaluations (Epic 5)
- Behavior (Epic 6)
- Tests/Exams (Epic 7)
- CMS (Epic 8)
- Notifications/Dashboards (Epic 9–10)
