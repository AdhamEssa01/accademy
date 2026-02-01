using Academy.Application.Abstractions.Assignments;
using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Dashboards;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ParentDashboardService : IParentDashboardService
{
    private const int SummaryDays = 30;
    private const int UpcomingAssignmentDays = 7;
    private const int RecentResultsDays = 30;
    private const int AnnouncementDays = 7;

    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IAssignmentService _assignmentService;
    private readonly IExamAttemptService _examAttemptService;

    public ParentDashboardService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext,
        IAssignmentService assignmentService,
        IExamAttemptService examAttemptService)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
        _assignmentService = assignmentService;
        _examAttemptService = examAttemptService;
    }

    public async Task<ParentDashboardDto> GetAsync(CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var guardianId = await _dbContext.Guardians
            .AsNoTracking()
            .Where(g => g.UserId == userId)
            .Select(g => g.Id)
            .FirstOrDefaultAsync(ct);

        if (guardianId == Guid.Empty)
        {
            return new ParentDashboardDto();
        }

        var students = await (from link in _dbContext.StudentGuardians.AsNoTracking()
                              join student in _dbContext.Students.AsNoTracking()
                                  on link.StudentId equals student.Id
                              where link.GuardianId == guardianId
                              select new { student.Id, student.FullName })
            .ToListAsync(ct);

        if (students.Count == 0)
        {
            return new ParentDashboardDto();
        }

        var studentIds = students.Select(s => s.Id).ToArray();
        var sinceUtc = DateTime.UtcNow.AddDays(-SummaryDays);

        var attendanceStats = await _dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.StudentId) && a.MarkedAtUtc >= sinceUtc)
            .GroupBy(a => a.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                Total = g.Count(),
                Attended = g.Count(a => a.Status == AttendanceStatus.Present
                    || a.Status == AttendanceStatus.Late
                    || a.Status == AttendanceStatus.Excused)
            })
            .ToListAsync(ct);

        var behaviorStats = await _dbContext.BehaviorEvents
            .AsNoTracking()
            .Where(b => studentIds.Contains(b.StudentId) && b.CreatedAtUtc >= sinceUtc)
            .GroupBy(b => b.StudentId)
            .Select(g => new { StudentId = g.Key, Points = g.Sum(x => x.Points) })
            .ToListAsync(ct);

        var evaluationStats = await _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId))
            .GroupBy(e => e.StudentId)
            .Select(g => new
            {
                StudentId = g.Key,
                LastScore = g.OrderByDescending(e => e.CreatedAtUtc)
                    .Select(e => e.TotalScore)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var attendanceLookup = attendanceStats.ToDictionary(x => x.StudentId, x => x);
        var behaviorLookup = behaviorStats.ToDictionary(x => x.StudentId, x => x.Points);
        var evaluationLookup = evaluationStats.ToDictionary(x => x.StudentId, x => x.LastScore);

        var summaries = students
            .Select(s =>
            {
                attendanceLookup.TryGetValue(s.Id, out var attendance);
                var attended = attendance?.Attended ?? 0;
                var total = attendance?.Total ?? 0;
                var rate = total > 0 ? Math.Round(attended / (decimal)total, 2) : 0m;

                return new ParentChildSummaryDto
                {
                    StudentId = s.Id,
                    FullName = s.FullName,
                    AttendanceRate = rate,
                    LastEvaluationScore = evaluationLookup.TryGetValue(s.Id, out var score) ? score : 0m,
                    BehaviorPoints = behaviorLookup.TryGetValue(s.Id, out var points) ? points : 0
                };
            })
            .OrderBy(s => s.FullName)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var upcomingPage = await _assignmentService.ListForParentAsync(
            today,
            today.AddDays(UpcomingAssignmentDays),
            new PagedRequest { Page = 1, PageSize = 20 },
            ct);

        var upcomingAssignments = upcomingPage.Items
            .Where(a => a.DueAtUtc.HasValue)
            .OrderBy(a => a.DueAtUtc)
            .Take(5)
            .ToList();

        var groupIds = await _dbContext.Enrollments
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId)
                && (e.EndDate == null || e.EndDate >= today))
            .Select(e => e.GroupId)
            .Distinct()
            .ToListAsync(ct);

        var announcementsQuery = _dbContext.Announcements
            .AsNoTracking()
            .Where(a => a.PublishedAtUtc >= DateTime.UtcNow.AddDays(-AnnouncementDays));

        if (groupIds.Count > 0)
        {
            announcementsQuery = announcementsQuery.Where(a => a.Audience == AnnouncementAudience.AllParents
                || (a.Audience == AnnouncementAudience.GroupParents && groupIds.Contains(a.GroupId!.Value)));
        }
        else
        {
            announcementsQuery = announcementsQuery.Where(a => a.Audience == AnnouncementAudience.AllParents);
        }

        var newAnnouncementsCount = await announcementsQuery.CountAsync(ct);

        var recentResultsPage = await _examAttemptService.ParentListMyChildrenAsync(
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-RecentResultsDays)),
            null,
            new PagedRequest { Page = 1, PageSize = 5 },
            ct);

        return new ParentDashboardDto
        {
            Children = summaries,
            UpcomingAssignments = upcomingAssignments,
            NewAnnouncementsCount = newAnnouncementsCount,
            RecentExamResults = recentResultsPage.Items
        };
    }
}
