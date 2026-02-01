using Academy.Application.Abstractions.Exams;
using Academy.Application.Abstractions.Security;
using Academy.Application.Contracts.Exams;
using Academy.Application.Exceptions;
using Academy.Domain;
using Academy.Infrastructure.Data;
using Academy.Shared.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Academy.Infrastructure.Services;

public sealed class ExamService : IExamService
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUserContext;

    public ExamService(
        AppDbContext dbContext,
        ITenantGuard tenantGuard,
        ICurrentUserContext currentUserContext)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResponse<ExamDto>> ListAsync(PagedRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var page = await _dbContext.Exams
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAtUtc)
            .ToPagedResponseAsync(request.Page, request.PageSize, ct);

        return await MapPagedAsync(page, ct);
    }

    public async Task<ExamDto> GetAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var exam = await _dbContext.Exams
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam is null)
        {
            throw new NotFoundException();
        }

        var questions = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => q.ExamId == exam.Id)
            .OrderBy(q => q.SortOrder)
            .Select(q => new ExamQuestionDto
            {
                Id = q.Id,
                QuestionId = q.QuestionId,
                Points = q.Points,
                SortOrder = q.SortOrder
            })
            .ToListAsync(ct);

        return Map(exam, questions);
    }

    public async Task<ExamDto> CreateAsync(CreateExamRequest request, CancellationToken ct)
    {
        var academyId = _tenantGuard.GetAcademyIdOrThrow();
        var userId = _currentUserContext.UserId ?? throw new ForbiddenException();

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            Title = request.Title,
            Type = request.Type,
            DurationMinutes = request.DurationMinutes,
            ShuffleQuestions = request.ShuffleQuestions,
            ShuffleOptions = request.ShuffleOptions,
            ShowResultsAfterSubmit = request.ShowResultsAfterSubmit,
            CreatedByUserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Exams.Add(exam);
        await _dbContext.SaveChangesAsync(ct);

        return Map(exam, Array.Empty<ExamQuestionDto>());
    }

    public async Task<ExamDto> UpdateAsync(Guid id, UpdateExamRequest request, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var exam = await _dbContext.Exams
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam is null)
        {
            throw new NotFoundException();
        }

        exam.Title = request.Title;
        exam.Type = request.Type;
        exam.DurationMinutes = request.DurationMinutes;
        exam.ShuffleQuestions = request.ShuffleQuestions;
        exam.ShuffleOptions = request.ShuffleOptions;
        exam.ShowResultsAfterSubmit = request.ShowResultsAfterSubmit;

        await _dbContext.SaveChangesAsync(ct);

        var questions = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => q.ExamId == exam.Id)
            .OrderBy(q => q.SortOrder)
            .Select(q => new ExamQuestionDto
            {
                Id = q.Id,
                QuestionId = q.QuestionId,
                Points = q.Points,
                SortOrder = q.SortOrder
            })
            .ToListAsync(ct);

        return Map(exam, questions);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var exam = await _dbContext.Exams
            .FirstOrDefaultAsync(e => e.Id == id, ct);

        if (exam is null)
        {
            throw new NotFoundException();
        }

        _dbContext.Exams.Remove(exam);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<ExamQuestionDto>> UpdateQuestionsAsync(
        Guid examId,
        UpdateExamQuestionsRequest request,
        CancellationToken ct)
    {
        _tenantGuard.EnsureAcademyScopeOrThrow();

        var examExists = await _dbContext.Exams
            .AnyAsync(e => e.Id == examId, ct);

        if (!examExists)
        {
            throw new NotFoundException();
        }

        var questionIds = request.Questions
            .Select(q => q.QuestionId)
            .ToArray();

        if (questionIds.Length != questionIds.Distinct().Count())
        {
            throw new ArgumentException("Duplicate questions are not allowed.");
        }

        if (questionIds.Length > 0)
        {
            var existingCount = await _dbContext.Questions
                .Where(q => questionIds.Contains(q.Id))
                .Select(q => q.Id)
                .Distinct()
                .CountAsync(ct);

            if (existingCount != questionIds.Length)
            {
                throw new NotFoundException();
            }
        }

        var existing = await _dbContext.ExamQuestions
            .Where(q => q.ExamId == examId)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _dbContext.ExamQuestions.RemoveRange(existing);
        }

        var academyId = _tenantGuard.GetAcademyIdOrThrow();

        var newItems = request.Questions.Select(item => new ExamQuestion
        {
            Id = Guid.NewGuid(),
            AcademyId = academyId,
            ExamId = examId,
            QuestionId = item.QuestionId,
            Points = item.Points,
            SortOrder = item.SortOrder
        }).ToList();

        if (newItems.Count > 0)
        {
            _dbContext.ExamQuestions.AddRange(newItems);
        }

        await _dbContext.SaveChangesAsync(ct);

        return newItems
            .OrderBy(q => q.SortOrder)
            .Select(MapQuestion)
            .ToList();
    }

    private async Task<PagedResponse<ExamDto>> MapPagedAsync(
        PagedResponse<Exam> page,
        CancellationToken ct)
    {
        if (page.Items.Count == 0)
        {
            return new PagedResponse<ExamDto>(Array.Empty<ExamDto>(), page.Page, page.PageSize, page.Total);
        }

        var examIds = page.Items.Select(e => e.Id).ToArray();

        var questions = await _dbContext.ExamQuestions
            .AsNoTracking()
            .Where(q => examIds.Contains(q.ExamId))
            .OrderBy(q => q.SortOrder)
            .Select(q => new
            {
                q.ExamId,
                Item = new ExamQuestionDto
                {
                    Id = q.Id,
                    QuestionId = q.QuestionId,
                    Points = q.Points,
                    SortOrder = q.SortOrder
                }
            })
            .ToListAsync(ct);

        var lookup = questions
            .GroupBy(q => q.ExamId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Item).ToList());

        var mapped = page.Items.Select(exam =>
        {
            var items = lookup.TryGetValue(exam.Id, out var list)
                ? list
                : new List<ExamQuestionDto>();

            return Map(exam, items);
        }).ToList();

        return new PagedResponse<ExamDto>(mapped, page.Page, page.PageSize, page.Total);
    }

    private static ExamDto Map(Exam exam, IReadOnlyList<ExamQuestionDto> questions)
        => new()
        {
            Id = exam.Id,
            AcademyId = exam.AcademyId,
            Title = exam.Title,
            Type = exam.Type,
            DurationMinutes = exam.DurationMinutes,
            ShuffleQuestions = exam.ShuffleQuestions,
            ShuffleOptions = exam.ShuffleOptions,
            ShowResultsAfterSubmit = exam.ShowResultsAfterSubmit,
            CreatedByUserId = exam.CreatedByUserId,
            CreatedAtUtc = exam.CreatedAtUtc,
            Questions = questions
        };

    private static ExamQuestionDto MapQuestion(ExamQuestion question)
        => new()
        {
            Id = question.Id,
            QuestionId = question.QuestionId,
            Points = question.Points,
            SortOrder = question.SortOrder
        };
}
