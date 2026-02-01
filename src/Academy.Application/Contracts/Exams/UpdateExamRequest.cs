using Academy.Domain;

namespace Academy.Application.Contracts.Exams;

public sealed class UpdateExamRequest
{
    public string Title { get; set; } = string.Empty;

    public ExamType Type { get; set; }

    public int DurationMinutes { get; set; }

    public bool ShuffleQuestions { get; set; }

    public bool ShuffleOptions { get; set; }

    public bool ShowResultsAfterSubmit { get; set; }
}
