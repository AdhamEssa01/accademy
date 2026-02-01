namespace Academy.Domain;

public sealed class ExamQuestion : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid ExamId { get; set; }

    public Guid QuestionId { get; set; }

    public int Points { get; set; }

    public int SortOrder { get; set; }
}
