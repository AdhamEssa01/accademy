namespace Academy.Application.Contracts.Exams;

public sealed class ExamQuestionDto
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public int Points { get; set; }

    public int SortOrder { get; set; }
}
