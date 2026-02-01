using Academy.Domain;

namespace Academy.Application.Contracts.Questions;

public sealed class QuestionDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid? ProgramId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? LevelId { get; set; }

    public QuestionType Type { get; set; }

    public string Text { get; set; } = string.Empty;

    public QuestionDifficulty Difficulty { get; set; }

    public string? Tags { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public IReadOnlyList<QuestionOptionDto> Options { get; set; } = Array.Empty<QuestionOptionDto>();
}
