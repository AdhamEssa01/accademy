namespace Academy.Domain;

public sealed class ExamAssignment : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid ExamId { get; set; }

    public Guid? GroupId { get; set; }

    public Guid? StudentId { get; set; }

    public DateTime OpenAtUtc { get; set; }

    public DateTime CloseAtUtc { get; set; }

    public int AttemptsAllowed { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
