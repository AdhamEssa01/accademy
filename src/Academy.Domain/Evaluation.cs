namespace Academy.Domain;

public sealed class Evaluation : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid StudentId { get; set; }

    public Guid TemplateId { get; set; }

    public Guid? SessionId { get; set; }

    public Guid EvaluatedByUserId { get; set; }

    public string? Notes { get; set; }

    public decimal TotalScore { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
