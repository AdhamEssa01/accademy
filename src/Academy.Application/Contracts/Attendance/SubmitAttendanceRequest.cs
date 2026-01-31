namespace Academy.Application.Contracts.Attendance;

public sealed class SubmitAttendanceRequest
{
    public List<AttendanceItemRequest> Items { get; set; } = new();
}
