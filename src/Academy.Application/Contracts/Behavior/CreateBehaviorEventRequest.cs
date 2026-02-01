using Academy.Domain;

namespace Academy.Application.Contracts.Behavior;

public sealed class CreateBehaviorEventRequest
{
    public Guid StudentId { get; set; }

    public Guid? SessionId { get; set; }

    public BehaviorEventType Type { get; set; }

    public int Points { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string? Note { get; set; }
}
