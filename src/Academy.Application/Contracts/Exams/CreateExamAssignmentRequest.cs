namespace Academy.Application.Contracts.Exams;

public sealed class CreateExamAssignmentRequest
{
    public Guid? GroupId { get; set; }

    public Guid? StudentId { get; set; }

    public DateTime OpenAtUtc { get; set; }

    public DateTime CloseAtUtc { get; set; }

    public int AttemptsAllowed { get; set; }
}
