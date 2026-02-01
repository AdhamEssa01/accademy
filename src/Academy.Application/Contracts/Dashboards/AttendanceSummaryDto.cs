namespace Academy.Application.Contracts.Dashboards;

public sealed class AttendanceSummaryDto
{
    public int Present { get; set; }

    public int Absent { get; set; }

    public int Late { get; set; }

    public int Excused { get; set; }

    public int Total { get; set; }
}
