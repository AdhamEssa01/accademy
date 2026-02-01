namespace Academy.Application.Contracts.Exams;

public sealed class ExamResultDto
{
    public Guid AttemptId { get; set; }

    public Guid AssignmentId { get; set; }

    public Guid ExamId { get; set; }

    public Guid StudentId { get; set; }

    public DateTime StartedAtUtc { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public decimal TotalScore { get; set; }
}
