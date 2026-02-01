using System.Text.Json;
using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamGradingService : IExamGradingService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;

    public ExamGradingService(AppDbContext dbContext, ITenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task GradeAttemptAsync(Guid attemptId, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var attempt = await _dbContext.ExamAttempts
            .FirstOrDefaultAsync(a => a.Id == attemptId, ct);

        if (attempt is null)
        {
            throw new NotFoundException();
        }

        if (attempt.Status != ExamAttemptStatus.Submitted)
        {
            throw new ArgumentException("Attempt must be submitted before grading.");
        }

        var assignment = await _dbContext.ExamAssignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attempt.AssignmentId, ct);

        if (assignment is null)
        {
            throw new NotFoundException();
        }

        var examQuestions = await (from examQuestion in _dbContext.ExamQuestions.AsNoTracking()
                                   join question in _dbContext.Questions.AsNoTracking()
                                       on examQuestion.QuestionId equals question.Id
                                   where examQuestion.ExamId == assignment.ExamId
                                   select new
                                   {
                                       examQuestion.QuestionId,
                                       examQuestion.Points,
                                       question.Type
                                   })
            .ToListAsync(ct);

        var questionIds = examQuestions.Select(q => q.QuestionId).ToArray();

        var options = await _dbContext.QuestionOptions
            .AsNoTracking()
            .Where(o => questionIds.Contains(o.QuestionId))
            .ToListAsync(ct);

        var optionsLookup = options
            .GroupBy(o => o.QuestionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var answers = await _dbContext.AttemptAnswers
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync(ct);

        var answerLookup = answers.ToDictionary(a => a.QuestionId, a => a);

        var totalScore = 0m;
        var allAutoGradable = examQuestions.All(q => q.Type is QuestionType.MCQ
            or QuestionType.TrueFalse
            or QuestionType.FillBlank);

        foreach (var question in examQuestions)
        {
            if (!answerLookup.TryGetValue(question.QuestionId, out var answer))
            {
                continue;
            }

            if (question.Type is QuestionType.MCQ or QuestionType.TrueFalse)
            {
                var selectedOptionId = TryGetOptionId(answer.AnswerJson);
                var correctOptions = optionsLookup.TryGetValue(question.QuestionId, out var list)
                    ? list.Where(o => o.IsCorrect).Select(o => o.Id).ToHashSet()
                    : new HashSet<Guid>();

                var isCorrect = selectedOptionId.HasValue && correctOptions.Contains(selectedOptionId.Value);
                answer.IsCorrect = isCorrect;
                answer.Score = isCorrect ? question.Points : 0m;
                totalScore += answer.Score ?? 0m;
            }
            else if (question.Type == QuestionType.FillBlank)
            {
                var submitted = TryGetTextValue(answer.AnswerJson);
                var correctTexts = optionsLookup.TryGetValue(question.QuestionId, out var list)
                    ? list.Where(o => o.IsCorrect).Select(o => o.Text.Trim()).ToArray()
                    : Array.Empty<string>();

                var isCorrect = submitted is not null
                    && correctTexts.Any(t => string.Equals(t, submitted, StringComparison.OrdinalIgnoreCase));

                answer.IsCorrect = isCorrect;
                answer.Score = isCorrect ? question.Points : 0m;
                totalScore += answer.Score ?? 0m;
            }
            else
            {
                answer.IsCorrect = null;
                answer.Score = null;
            }
        }

        attempt.TotalScore = totalScore;

        if (allAutoGradable)
        {
            attempt.Status = ExamAttemptStatus.Graded;
        }

        await _dbContext.SaveChangesAsync(ct);
    }

    private static Guid? TryGetOptionId(string payload)
    {
        var trimmed = payload.Trim();
        if (Guid.TryParse(trimmed.Trim('"'), out var parsed))
        {
            return parsed;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("optionId", out var optionId)
                && Guid.TryParse(optionId.GetString(), out parsed))
            {
                return parsed;
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private static string? TryGetTextValue(string payload)
    {
        var trimmed = payload.Trim().Trim('"');
        if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("{", StringComparison.Ordinal))
        {
            return trimmed;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            if (document.RootElement.TryGetProperty("value", out var value))
            {
                return value.GetString()?.Trim();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }
}
