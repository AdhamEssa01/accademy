namespace Academy.Domain;

public sealed class ExamAttempt : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid AssignmentId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public ExamAttemptStatus Status { get; set; }

    public decimal TotalScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
