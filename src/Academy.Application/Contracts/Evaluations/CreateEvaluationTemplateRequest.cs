namespace Academy.Application.Contracts.Evaluations;

public sealed class CreateEvaluationTemplateRequest
{
    public Guid? ProgramId { get; set; }

    public Guid? CourseId { get; set; }

    public Guid? LevelId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}
