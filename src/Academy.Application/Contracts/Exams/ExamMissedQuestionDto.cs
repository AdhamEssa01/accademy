namespace Academy.Application.Contracts.Exams;

public sealed class ExamMissedQuestionDto
{
    public Guid QuestionId { get; set; }

    public int MissCount { get; set; }
}
