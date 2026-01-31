using Academy.Application.Contracts.Enrollments;

namespace Academy.Application.Abstractions.Enrollments;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request, CancellationToken ct);

    Task<EnrollmentDto> EndAsync(Guid enrollmentId, EndEnrollmentRequest request, CancellationToken ct);

    Task<IReadOnlyList<EnrollmentDto>> ListByStudentAsync(Guid studentId, CancellationToken ct);

    Task<IReadOnlyList<EnrollmentDto>> ListByGroupAsync(Guid groupId, CancellationToken ct);
}
