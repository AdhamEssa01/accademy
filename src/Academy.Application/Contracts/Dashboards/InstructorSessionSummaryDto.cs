namespace Academy.Application.Contracts.Dashboards;

public sealed class InstructorSessionSummaryDto
{
    public Guid SessionId { get; set; }

    public Guid GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public DateTime StartsAtUtc { get; set; }

    public int DurationMinutes { get; set; }
}
