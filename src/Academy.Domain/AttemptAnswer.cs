namespace Academy.Domain;

public sealed class AttemptAnswer : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid AttemptId { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerJson { get; set; } = string.Empty;

    public bool? IsCorrect { get; set; }

    public decimal? Score { get; set; }

    public string? Feedback { get; set; }
}
