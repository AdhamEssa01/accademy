using Academy.Application.Abstractions.Security;
using Academy.Application.Abstractions.Students;
using Academy.Application.Contracts.Students;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class StudentRiskService : IStudentRiskService
{
    private const int AbsenceThreshold = 3;
    private const int NegativePointsThreshold = -5;
    private const decimal EvaluationScoreThreshold = 60m;

    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public StudentRiskService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<PagedResponse<StudentRiskDto>> GetRiskListAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var (fromUtc, toUtc) = ResolveRange(from, to);

        var studentPage = await _dbContext.Students
            .AsNoTracking()
            .OrderBy(s => s.FullName)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        if (studentPage.Items.Count == 0)
        {
            return new PagedResponse<StudentRiskDto>(Array.Empty<StudentRiskDto>(), request.Page, request.PageSize, studentPage.Total);
        }

        var studentIds = studentPage.Items.Select(s => s.Id).ToArray();

        var absences = await _dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.StudentId)
                && a.Status == AttendanceStatus.Absent
                && a.MarkedAtUtc >= fromUtc
                && a.MarkedAtUtc <= toUtc)
            .GroupBy(a => a.StudentId)
            .Select(g => new { StudentId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var absenceLookup = absences.ToDictionary(x => x.StudentId, x => x.Count);

        var behaviorPoints = await _dbContext.BehaviorEvents
            .AsNoTracking()
            .Where(b => studentIds.Contains(b.StudentId)
                && b.CreatedAtUtc >= fromUtc
                && b.CreatedAtUtc <= toUtc)
            .GroupBy(b => b.StudentId)
            .Select(g => new { StudentId = g.Key, Points = g.Sum(x => x.Points) })
            .ToListAsync(ct);

        var behaviorLookup = behaviorPoints.ToDictionary(x => x.StudentId, x => x.Points);

        var evaluations = await _dbContext.Evaluations
            .AsNoTracking()
            .Where(e => studentIds.Contains(e.StudentId)
                && e.CreatedAtUtc >= fromUtc
                && e.CreatedAtUtc <= toUtc)
            .GroupBy(e => e.StudentId)
            .Select(g => new { StudentId = g.Key, Average = g.Average(x => x.TotalScore), Count = g.Count() })
            .ToListAsync(ct);

        var evaluationLookup = evaluations.ToDictionary(x => x.StudentId, x => x);

        var items = studentPage.Items.Select(student =>
        {
            var absenceCount = absenceLookup.TryGetValue(student.Id, out var count) ? count : 0;
            var behaviorTotal = behaviorLookup.TryGetValue(student.Id, out var points) ? points : 0;

            var averageScore = 0m;
            var hasScores = false;
            if (evaluationLookup.TryGetValue(student.Id, out var eval))
            {
                hasScores = eval.Count > 0;
                averageScore = eval.Count > 0 ? eval.Average : 0m;
            }

            var isAtRisk = absenceCount >= AbsenceThreshold
                || behaviorTotal <= NegativePointsThreshold
                || (hasScores && averageScore < EvaluationScoreThreshold);

            return new StudentRiskDto
            {
                StudentId = student.Id,
                FullName = student.FullName,
                Absences = absenceCount,
                BehaviorPoints = behaviorTotal,
                AverageEvaluationScore = Math.Round(averageScore, 2),
                IsAtRisk = isAtRisk
            };
        }).ToList();

        return new PagedResponse<StudentRiskDto>(items, studentPage.Page, studentPage.PageSize, studentPage.Total);
    }

    private static (DateTime FromUtc, DateTime ToUtc) ResolveRange(DateOnly? from, DateOnly? to)
    {
        var endDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var startDate = from ?? endDate.AddDays(-30);

        if (startDate > endDate)
        {
            throw new ArgumentException("From date cannot be later than to date.");
        }

        return (startDate.ToDateTime(TimeOnly.MinValue), endDate.ToDateTime(TimeOnly.MaxValue));
    }
}
