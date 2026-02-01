namespace Academy.Domain;

public sealed class BehaviorEvent : IAcademyScoped
{
    public Guid Id { get; set; }

    public Guid AcademyId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? SessionId { get; set; }

    public BehaviorEventType Type { get; set; }

    public int Points { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Note { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
