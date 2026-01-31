namespace Academy.Application.Contracts.Students;

public sealed class UpdateStudentRequest
{
    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Notes { get; set; }
}
