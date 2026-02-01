using Academy.Domain;

namespace Academy.Application.Contracts.Exams;

public sealed class ExamAttemptDto
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public Guid StudentId { get; set; }

    public ExamAttemptStatus Status { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public decimal TotalScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
