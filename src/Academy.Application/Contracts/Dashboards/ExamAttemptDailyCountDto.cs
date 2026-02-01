namespace Academy.Application.Contracts.Dashboards;

public sealed class ExamAttemptDailyCountDto
{
    public DateOnly Date { get; set; }

    public int Count { get; set; }
}
