using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Exams;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Security;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamManualGradingService : IExamManualGradingService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public ExamManualGradingService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task GradeAnswerAsync(Guid answerId, GradeAttemptAnswerRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var answer = await _dbContext.AttemptAnswers
            .FirstOrDefaultAsync(a => a.Id == answerId, ct);

        if (answer is null)
        {
            throw new NotFoundException();
        }

        var attempt = await _dbContext.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == answer.AttemptId, ct);

        if (attempt is null)
        {
            throw new NotFoundException();
        }

        var assignment = await _dbContext.ExamAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attempt.AssignmentId, ct);

        if (assignment is null)
        {
            throw new NotFoundException();
        }

        var examQuestion = await _dbContext.ExamQuestions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.ExamId == assignment.ExamId && q.QuestionId == answer.QuestionId, ct);

        if (examQuestion is null)
        {
            throw new NotFoundException();
        }

        await EnsureGradingAccessAsync(assignment, ct);

        if (request.Score > examQuestion.Points)
        {
            throw new ArgumentException("Score exceeds question points.");
        }

        answer.Score = request.Score;
        answer.Feedback = request.Feedback;
        answer.IsCorrect = null;

        await UpdateAttemptTotalsAsync(attempt, assignment.ExamId, ct);

        await _dbContext.SaveChangesAsync(ct);
    }

    private async Task EnsureGradingAccessAsync(ExamAssignment assignment, CancellationToken ct)
    {
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();
        var roles = _currentUserContext.Roles;

        if (roles.Contains(Roles.Admin))
        {
            return;
        }

        if (!roles.Contains(Roles.Instructor))
        {
            throw new ForbiddenException();
        }

        if (!assignment.GroupId.HasValue)
        {
            throw new ForbiddenException();
        }

        var group = await _dbContext.Groups
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == assignment.GroupId.Value, ct);

        if (group is null || group.InstructorUserId != userId)
        {
            throw new ForbiddenException();
        }
    }

    private async Task UpdateAttemptTotalsAsync(ExamAttempt attempt, Guid examId, CancellationToken ct)
    {
        var questionIds = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => q.ExamId == examId)
            .Select(q => q.QuestionId)
            .ToListAsync(ct);

        var gradedScores = await _dbContext.AttemptAnswers
            .Where(a => a.AttemptId == attempt.Id && a.Score.HasValue && questionIds.Contains(a.QuestionId))
            .ToListAsync(ct);

        attempt.TotalScore = gradedScores.Sum(a => a.Score ?? 0m);

        if (gradedScores.Select(a => a.QuestionId).Distinct().Count() == questionIds.Count)
        {
            attempt.Status = ExamAttemptStatus.Graded;
        }
    }
}
