namespace Academy.Application.Contracts.Students;

public sealed class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Notes { get; set; }
}
