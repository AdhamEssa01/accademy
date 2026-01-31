using Academy.Application.Contracts.Attendance;

namespace Academy.Application.Abstractions.Attendance;

public interface IAttendanceService
{
    Task<IReadOnlyList<AttendanceRecordDto>> SubmitForSessionAsync(
        Guid sessionId,
        SubmitAttendanceRequest request,
        CancellationToken ct);

    Task<IReadOnlyList<AttendanceRecordDto>> ListForSessionAsync(Guid sessionId, CancellationToken ct);
}
