using Academy.Application.Contracts.Attendance;
using Academy.Shared.Pagination;
using Academy.Domain;

namespace Academy.Application.Abstractions.Attendance;

public interface IAttendanceQueryService
{
    Task<PagedResponse<AttendanceRecordDto>> ListAsync(
        Guid? groupId,
        Guid? studentId,
        DateOnly? from,
        DateOnly? to,
        AttendanceStatus? status,
        PagedRequest request,
        CancellationToken ct);

    Task<PagedResponse<AttendanceRecordDto>> ParentListForMyChildrenAsync(
        DateOnly? from,
        DateOnly? to,
        PagedRequest request,
        CancellationToken ct);
}
