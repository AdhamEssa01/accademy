namespace Academy.Application.Contracts.Exams;

public sealed class ExamStatsDto
{
    public Guid ExamId { get; set; }

    public int AttemptsCount { get; set; }

    public decimal AverageScore { get; set; }

    public IReadOnlyList<ExamScoreBucketDto> ScoreDistribution { get; set; } = Array.Empty<ExamScoreBucketDto>();

    public IReadOnlyList<ExamMissedQuestionDto> MostMissedQuestions { get; set; } = Array.Empty<ExamMissedQuestionDto>();
}
