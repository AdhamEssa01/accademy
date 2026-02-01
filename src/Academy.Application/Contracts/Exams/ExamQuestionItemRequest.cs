namespace Academy.Application.Contracts.Exams;

public sealed class ExamQuestionItemRequest
{
    public Guid QuestionId { get; set; }

    public int Points { get; set; }

    public int SortOrder { get; set; }
}
