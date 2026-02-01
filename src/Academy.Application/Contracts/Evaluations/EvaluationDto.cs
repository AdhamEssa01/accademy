namespace Academy.Application.Contracts.Evaluations;

public sealed class EvaluationDto
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

    public IReadOnlyList<EvaluationItemDto> Items { get; set; } = Array.Empty<EvaluationItemDto>();
}
