namespace Academy.Domain;

public sealed class EvaluationItem : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid EvaluationId { get; set; }

    public Guid CriterionId { get; set; }

    public decimal Score { get; set; }

    public string? Comment { get; set; }
}
