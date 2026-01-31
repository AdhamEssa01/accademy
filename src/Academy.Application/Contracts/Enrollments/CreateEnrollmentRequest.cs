namespace Academy.Application.Contracts.Enrollments;

public sealed class CreateEnrollmentRequest
{
    public Guid StudentId { get; set; }

    public Guid GroupId { get; set; }

    public DateOnly StartDate { get; set; }
}
