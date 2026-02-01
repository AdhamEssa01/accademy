namespace Academy.Application.Contracts.Exams;

public sealed class ExamScoreBucketDto
{
    public string Range { get; set; } = string.Empty;

    public int Count { get; set; }
}
