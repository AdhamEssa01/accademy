using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Exams;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamAttemptService : IExamAttemptService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public ExamAttemptService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<ExamAttemptDto> StartAsync(Guid assignmentId, StartExamAttemptRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var assignment = await _dbContext.ExamAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == assignmentId, ct);

        if (assignment is null)
        {
            throw new NotFoundException();
        }

        var now = DateTime.UtcNow;
        if (now < assignment.OpenAtUtc || now > assignment.CloseAtUtc)
        {
            throw new ArgumentException("Assignment is not open.");
        }

        var studentExists = await _dbContext.Students
            .AnyAsync(s => s.Id == request.StudentId, ct);

        if (!studentExists)
        {
            throw new NotFoundException();
        }

        await EnsureStudentAssignmentAccessAsync(assignment, request.StudentId, ct);

        var attemptCount = await _dbContext.ExamAttempts
            .CountAsync(a => a.AssignmentId == assignmentId && a.StudentId == request.StudentId, ct);

        if (attemptCount >= assignment.AttemptsAllowed)
        {
            throw new ArgumentException("Attempts limit reached.");
        }

        var inProgress = await _dbContext.ExamAttempts
            .AnyAsync(a => a.AssignmentId == assignmentId
                && a.StudentId == request.StudentId
                && a.Status == ExamAttemptStatus.InProgress, ct);

        if (inProgress)
        {
            throw new ArgumentException("An attempt is already in progress.");
        }

        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var attempt = new ExamAttempt
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            AssignmentId = assignmentId,
            StudentId = request.StudentId,
            StartedAtUtc = now,
            Status = ExamAttemptStatus.InProgress,
            TotalScore = 0m,
            CreatedAtUtc = now
        };

        _dbContext.ExamAttempts.Add(attempt);
        await _dbContext.SaveChangesAsync(ct);

        return Map(attempt);
    }

    public async Task SaveAnswersAsync(Guid attemptId, SaveAttemptAnswersRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var attempt = await _dbContext.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null)
        {
            throw new NotFoundException();
        }

        if (attempt.Status != ExamAttemptStatus.InProgress)
        {
            throw new ArgumentException("Attempt is not in progress.");
        }

        var assignment = await _dbContext.ExamAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attempt.AssignmentId, ct);

        if (assignment is null)
        {
            throw new NotFoundException();
        }

        var distinctQuestions = request.Answers.Select(a => a.QuestionId).Distinct().ToArray();
        if (distinctQuestions.Length != request.Answers.Count)
        {
            throw new ArgumentException("Duplicate answers are not allowed.");
        }

        var validQuestionIds = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => q.ExamId == assignment.ExamId)
            .Select(q => q.QuestionId)
            .ToListAsync(ct);

        if (distinctQuestions.Any(id => !validQuestionIds.Contains(id)))
        {
            throw new ArgumentException("Answer contains invalid question.");
        }

        var existing = await _dbContext.AttemptAnswers
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync(ct);

        var existingLookup = existing.ToDictionary(a => a.QuestionId, a => a);
        var academyId = attempt.AcademyId;

        foreach (var answer in request.Answers)
        {
            if (existingLookup.TryGetValue(answer.QuestionId, out var stored))
            {
                stored.AnswerJson = answer.AnswerJson;
            }
            else
            {
                _dbContext.AttemptAnswers.Add(new AttemptAnswer
                {
                    Id = Guid.NewGuid(),
                    AcademyId = academyId,
                    AttemptId = attemptId,
                    QuestionId = answer.QuestionId,
                    AnswerJson = answer.AnswerJson
                });
            }
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<ExamAttemptDto> SubmitAsync(Guid attemptId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var attempt = await _dbContext.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null)
        {
            throw new NotFoundException();
        }

        if (attempt.Status != ExamAttemptStatus.InProgress)
        {
            throw new ArgumentException("Attempt is not in progress.");
        }

        attempt.Status = ExamAttemptStatus.Submitted;
        attempt.SubmittedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(ct);

        return Map(attempt);
    }

    public async Task<PagedResponse<ExamResultDto>> ParentListMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct)
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
            return new PagedResponse<ExamResultDto>(Array.Empty<ExamResultDto>(), request.Page, request.PageSize, 0);
        }

        var studentIds = await _dbContext.StudentGuardians
            .AsNoTracking()
            .Where(sg => sg.GuardianId == guardianId)
            .Select(sg => sg.StudentId)
            .Distinct()
            .ToListAsync(ct);

        if (studentIds.Count == 0)
        {
            return new PagedResponse<ExamResultDto>(Array.Empty<ExamResultDto>(), request.Page, request.PageSize, 0);
        }

        var query = from attempt in _dbContext.ExamAttempts.AsNoTracking()
                    join assignment in _dbContext.ExamAssignments.AsNoTracking()
                        on attempt.AssignmentId equals assignment.Id
                    where studentIds.Contains(attempt.StudentId)
                    select new ExamResultDto
                    {
                        AttemptId = attempt.Id,
                        AssignmentId = attempt.AssignmentId,
                        ExamId = assignment.ExamId,
                        StudentId = attempt.StudentId,
                        StartedAtUtc = attempt.StartedAtUtc,
                        SubmittedAtUtc = attempt.SubmittedAtUtc,
                        TotalScore = attempt.TotalScore
                    };

        query = ApplyDateRange(query, from, to);

        return await query
            .OrderByDescending(r => r.SubmittedAtUtc ?? r.StartedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    public async Task<PagedResponse<ExamResultDto>> ListForExamAsync(
        Guid examId,
        PagedRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var examExists = await _dbContext.Exams.AnyAsync(e => e.Id == examId, ct);
        if (!examExists)
        {
            throw new NotFoundException();
        }

        var query = from attempt in _dbContext.ExamAttempts.AsNoTracking()
                    join assignment in _dbContext.ExamAssignments.AsNoTracking()
                        on attempt.AssignmentId equals assignment.Id
                    where assignment.ExamId == examId
                    select new ExamResultDto
                    {
                        AttemptId = attempt.Id,
                        AssignmentId = attempt.AssignmentId,
                        ExamId = assignment.ExamId,
                        StudentId = attempt.StudentId,
                        StartedAtUtc = attempt.StartedAtUtc,
                        SubmittedAtUtc = attempt.SubmittedAtUtc,
                        TotalScore = attempt.TotalScore
                    };

        return await query
            .OrderByDescending(r => r.SubmittedAtUtc ?? r.StartedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);
    }

    private async Task EnsureStudentAssignmentAccessAsync(
        ExamAssignment assignment,
        Guid studentId,
        CancellationToken ct)
    {
        if (assignment.StudentId.HasValue)
        {
            if (assignment.StudentId.Value != studentId)
            {
                throw new ForbiddenException();
            }

            return;
        }

        if (!assignment.GroupId.HasValue)
        {
            throw new ForbiddenException();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        var isEnrolled = await _dbContext.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.GroupId == assignment.GroupId.Value
                && e.StudentId == studentId
                && (e.EndDate == null || e.EndDate >= today), ct);

        if (!isEnrolled)
        {
            throw new ForbiddenException();
        }
    }

    private static IQueryable<ExamResultDto> ApplyDateRange(
        IQueryable<ExamResultDto> query,
        DateOnly? from,
        DateOnly? to)
    {
        if (from.HasValue)
        {
            var fromUtc = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(r => r.StartedAtUtc >= fromUtc);
        }

        if (to.HasValue)
        {
            var toUtc = to.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(r => (r.SubmittedAtUtc ?? r.StartedAtUtc) <= toUtc);
        }

        return query;
    }

    private static ExamAttemptDto Map(ExamAttempt attempt)
        => new()
        {
            Id = attempt.Id,
            AssignmentId = attempt.AssignmentId,
            StudentId = attempt.StudentId,
            Status = attempt.Status,
            StartedAtUtc = attempt.StartedAtUtc,
            SubmittedAtUtc = attempt.SubmittedAtUtc,
            TotalScore = attempt.TotalScore,
            CreatedAtUtc = attempt.CreatedAtUtc
        };
}
