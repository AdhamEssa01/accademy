namespace Academy.Application.Contracts.Sessions;

public sealed class CreateSessionRequest
{
    public Guid GroupId { get; set; }

    public Guid InstructorUserId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public int DurationMinutes { get; set; }

    public string? Notes { get; set; }
}
