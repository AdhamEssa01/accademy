using Academy.Application.Abstractions.Dashboards;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Dashboards;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class InstructorDashboardService : IInstructorDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public InstructorDashboardService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<InstructorDashboardDto> GetAsync(CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startUtc = today.ToDateTime(TimeOnly.MinValue);
        var endUtc = today.ToDateTime(TimeOnly.MaxValue);

        var sessions = await (from session in _dbContext.Sessions.AsNoTracking()
                              join group in _dbContext.Groups.AsNoTracking()
                                  on session.GroupId equals group.Id
                              where session.InstructorUserId == userId
                                  && session.StartsAtUtc >= startUtc
                                  && session.StartsAtUtc <= endUtc
                              orderby session.StartsAtUtc
                              select new InstructorSessionSummaryDto
                              {
                                  SessionId = session.Id,
                                  GroupId = group.Id,
                                  GroupName = group.Name,
                                  StartsAtUtc = session.StartsAtUtc,
                                  DurationMinutes = session.DurationMinutes
                              })
            .ToListAsync(ct);

        var pendingManual = await (from answer in _dbContext.AttemptAnswers.AsNoTracking()
                                   join attempt in _dbContext.ExamAttempts.AsNoTracking()
                                       on answer.AttemptId equals attempt.Id
                                   join assignment in _dbContext.ExamAssignments.AsNoTracking()
                                       on attempt.AssignmentId equals assignment.Id
                                   join group in _dbContext.Groups.AsNoTracking()
                                       on assignment.GroupId equals group.Id
                                   join question in _dbContext.Questions.AsNoTracking()
                                       on answer.QuestionId equals question.Id
                                   where group.InstructorUserId == userId
                                       && answer.Score == null
                                       && attempt.Status != ExamAttemptStatus.Graded
                                       && (question.Type == QuestionType.Essay || question.Type == QuestionType.FileUpload)
                                   select answer.Id)
            .CountAsync(ct);

        var recentEvaluations = await (from evaluation in _dbContext.Evaluations.AsNoTracking()
                                       join student in _dbContext.Students.AsNoTracking()
                                           on evaluation.StudentId equals student.Id
                                       where evaluation.EvaluatedByUserId == userId
                                       orderby evaluation.CreatedAtUtc descending
                                       select new InstructorEvaluationSummaryDto
                                       {
                                           EvaluationId = evaluation.Id,
                                           StudentId = student.Id,
                                           StudentName = student.FullName,
                                           TotalScore = evaluation.TotalScore,
                                           CreatedAtUtc = evaluation.CreatedAtUtc
                                       })
            .Take(5)
            .ToListAsync(ct);

        return new InstructorDashboardDto
        {
            TodaySessions = sessions,
            PendingManualGradingCount = pendingManual,
            RecentEvaluations = recentEvaluations
        };
    }
}
