using Academy.Domain;

namespace Academy.Application.Contracts.Attendance;

public sealed class AttendanceItemRequest
{
    public Guid StudentId { get; set; }

    public AttendanceStatus Status { get; set; }

    public string? Reason { get; set; }

    public string? Note { get; set; }
}
