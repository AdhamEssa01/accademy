using Academy.Domain;

namespace Academy.Application.Contracts.Questions;

public sealed class UpdateQuestionRequest
{
    public Guid? ProgramId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? LevelId { get; set; }

    public QuestionType Type { get; set; }

    public string Text { get; set; } = string.Empty;

    public QuestionDifficulty Difficulty { get; set; }

    public string? Tags { get; set; }

    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}
