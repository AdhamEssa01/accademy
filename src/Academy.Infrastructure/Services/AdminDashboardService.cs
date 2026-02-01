using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Abstractions.Security;
using Academy.Application.Abstractions.Students;
using Academy.Application.Contracts.Dashboards;
using Academy.Application.Contracts.Students;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly IStudentRiskService _studentRiskService;

    public AdminDashboardService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        IStudentRiskService studentRiskService)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _studentRiskService = studentRiskService;
    }

    public async Task<AdminDashboardDto> GetAsync(CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var attendance = await BuildAttendanceSummaryAsync(ct);
        var riskyStudents = await BuildRiskyStudentsAsync(ct);
        var pendingManual = await BuildPendingManualGradingCountAsync(ct);
        var examAttempts = await BuildExamAttemptsLast7DaysAsync(ct);

        return new AdminDashboardDto
        {
            AttendanceToday = attendance,
            RiskyStudents = riskyStudents,
            PendingManualGradingCount = pendingManual,
            ExamAttemptsLast7Days = examAttempts
        };
    }

    private async Task<AttendanceSummaryDto> BuildAttendanceSummaryAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startUtc = today.ToDateTime(TimeOnly.MinValue);
        var endUtc = today.ToDateTime(TimeOnly.MaxValue);

        var records = await _dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.MarkedAtUtc >= startUtc && a.MarkedAtUtc <= endUtc)
            .Select(a => a.Status)
            .ToListAsync(ct);

        var summary = new AttendanceSummaryDto
        {
            Present = records.Count(s => s == AttendanceStatus.Present),
            Absent = records.Count(s => s == AttendanceStatus.Absent),
            Late = records.Count(s => s == AttendanceStatus.Late),
            Excused = records.Count(s => s == AttendanceStatus.Excused)
        };

        summary.Total = summary.Present + summary.Absent + summary.Late + summary.Excused;
        return summary;
    }

    private async Task<IReadOnlyList<StudentRiskDto>> BuildRiskyStudentsAsync(CancellationToken ct)
    {
        var page = await _studentRiskService.GetRiskListAsync(null, null, new PagedRequest
        {
            Page = 1,
            PageSize = 20
        }, ct);

        return page.Items
            .Where(r => r.IsAtRisk)
            .Take(5)
            .ToList();
    }

    private async Task<int> BuildPendingManualGradingCountAsync(CancellationToken ct)
    {
        return await (from answer in _dbContext.AttemptAnswers.AsNoTracking()
                      join attempt in _dbContext.ExamAttempts.AsNoTracking()
                          on answer.AttemptId equals attempt.Id
                      join question in _dbContext.Questions.AsNoTracking()
                          on answer.QuestionId equals question.Id
                      where answer.Score == null
                          && attempt.Status != ExamAttemptStatus.Graded
                          && (question.Type == QuestionType.Essay || question.Type == QuestionType.FileUpload)
                      select answer.Id)
            .CountAsync(ct);
    }

    private async Task<IReadOnlyList<ExamAttemptDailyCountDto>> BuildExamAttemptsLast7DaysAsync(CancellationToken ct)
    {
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = endDate.AddDays(-6);
        var startUtc = startDate.ToDateTime(TimeOnly.MinValue);

        var attempts = await _dbContext.ExamAttempts
            .AsNoTracking()
            .Where(a => a.CreatedAtUtc >= startUtc)
            .GroupBy(a => a.CreatedAtUtc.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var lookup = attempts.ToDictionary(a => DateOnly.FromDateTime(a.Date), a => a.Count);
        var result = new List<ExamAttemptDailyCountDto>();

        for (var i = 0; i < 7; i++)
        {
            var date = startDate.AddDays(i);
            result.Add(new ExamAttemptDailyCountDto
            {
                Date = date,
                Count = lookup.TryGetValue(date, out var count) ? count : 0
            });
        }

        return result;
    }
}
