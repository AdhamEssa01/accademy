namespace Academy.Application.Contracts.Exams;

public sealed class AttemptAnswerRequest
{
    public Guid QuestionId { get; set; }

    public string AnswerJson { get; set; } = string.Empty;
}
