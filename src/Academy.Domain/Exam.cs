namespace Academy.Domain;

public sealed class Exam : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ExamType Type { get; set; }

    public int DurationMinutes { get; set; }

    public bool ShuffleQuestions { get; set; }

    public bool ShuffleOptions { get; set; }

    public bool ShowResultsAfterSubmit { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
