namespace Academy.Application.Contracts.Exams;

public sealed class GradeAttemptAnswerRequest
{
    public decimal Score { get; set; }

    public string? Feedback { get; set; }
}
