using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Exams;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamAnalyticsService : IExamAnalyticsService
{
    private static readonly (int Min, int Max)[] Buckets =
    [
        (0, 59),
        (60, 69),
        (70, 79),
        (80, 89),
        (90, 100)
    ];

    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public ExamAnalyticsService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<ExamStatsDto> GetStatsAsync(Guid examId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var examExists = await _dbContext.Exams.AnyAsync(e => e.Id == examId, ct);
        if (!examExists)
        {
            throw new NotFoundException();
        }

        var examQuestions = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => q.ExamId == examId)
            .Select(q => new { q.QuestionId, q.Points })
            .ToListAsync(ct);

        var maxScore = examQuestions.Sum(q => q.Points);

        var attempts = await (from attempt in _dbContext.ExamAttempts.AsNoTracking()
                              join assignment in _dbContext.ExamAssignments.AsNoTracking()
                                  on attempt.AssignmentId equals assignment.Id
                              where assignment.ExamId == examId
                              select new AttemptScoreSnapshot(attempt.Id, attempt.TotalScore))
            .ToListAsync(ct);

        var attemptsCount = attempts.Count;
        var averageScore = attemptsCount == 0
            ? 0m
            : Math.Round(attempts.Average(a => a.TotalScore), 2);

        var distribution = BuildDistribution(attempts, maxScore);
        var mostMissed = await BuildMostMissedAsync(examId, examQuestions.Select(q => q.QuestionId).ToArray(), attemptsCount, ct);

        return new ExamStatsDto
        {
            ExamId = examId,
            AttemptsCount = attemptsCount,
            AverageScore = averageScore,
            ScoreDistribution = distribution,
            MostMissedQuestions = mostMissed
        };
    }

    private static IReadOnlyList<ExamScoreBucketDto> BuildDistribution(
        IReadOnlyList<AttemptScoreSnapshot> attempts,
        int maxScore)
    {
        var buckets = Buckets.Select(b => new ExamScoreBucketDto
        {
            Range = $"{b.Min}-{b.Max}",
            Count = 0
        }).ToList();

        if (attempts.Count == 0)
        {
            return buckets;
        }

        foreach (var attempt in attempts)
        {
            var percent = maxScore == 0 ? 0 : (int)Math.Round((attempt.TotalScore / maxScore) * 100m, 0);
            percent = Math.Clamp(percent, 0, 100);

            var index = Array.FindIndex(Buckets, b => percent >= b.Min && percent <= b.Max);
            if (index >= 0)
            {
                buckets[index].Count++;
            }
        }

        return buckets;
    }

    private readonly record struct AttemptScoreSnapshot(Guid AttemptId, decimal TotalScore);

    private async Task<IReadOnlyList<ExamMissedQuestionDto>> BuildMostMissedAsync(
        Guid examId,
        Guid[] questionIds,
        int attemptsCount,
        CancellationToken ct)
    {
        if (questionIds.Length == 0 || attemptsCount == 0)
        {
            return Array.Empty<ExamMissedQuestionDto>();
        }

        var attemptIds = await (from attempt in _dbContext.ExamAttempts.AsNoTracking()
                                join assignment in _dbContext.ExamAssignments.AsNoTracking()
                                    on attempt.AssignmentId equals assignment.Id
                                where assignment.ExamId == examId
                                select attempt.Id)
            .ToListAsync(ct);

        if (attemptIds.Count == 0)
        {
            return Array.Empty<ExamMissedQuestionDto>();
        }

        var answers = await _dbContext.AttemptAnswers
            .AsNoTracking()
            .Where(a => attemptIds.Contains(a.AttemptId) && questionIds.Contains(a.QuestionId))
            .Select(a => new { a.QuestionId, a.IsCorrect, a.Score })
            .ToListAsync(ct);

        var correctCounts = answers
            .Where(a => a.IsCorrect == true || (a.Score.HasValue && a.Score.Value > 0))
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Count());

        var missed = questionIds.Select(q => new ExamMissedQuestionDto
        {
            QuestionId = q,
            MissCount = attemptsCount - (correctCounts.TryGetValue(q, out var count) ? count : 0)
        })
        .OrderByDescending(x => x.MissCount)
        .ThenBy(x => x.QuestionId)
        .Take(5)
        .ToList();

        return missed;
    }
}
