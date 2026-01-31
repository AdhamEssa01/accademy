namespace Academy.Application.Contracts.Auth;

public sealed class GoogleLoginRequest
{
    public string IdToken { get; set; } = string.Empty;

    public Guid? AcademyId { get; set; }
}