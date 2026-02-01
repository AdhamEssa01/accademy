namespace Academy.Application.Contracts.Exams;

public sealed class SaveAttemptAnswersRequest
{
    public List<AttemptAnswerRequest> Answers { get; set; } = new();
}
