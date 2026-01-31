namespace Academy.Application.Contracts.Enrollments;

public sealed class EnrollmentDto
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid StudentId { get; set; }

    public Guid GroupId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
