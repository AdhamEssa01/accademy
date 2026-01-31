namespace Academy.Application.Contracts.Sessions;

public sealed class UpdateSessionRequest
{
    public Guid InstructorUserId { get; set; }

    public DateTime StartsAtUtc { get; set; }

    public int DurationMinutes { get; set; }

    public string? Notes { get; set; }
}
