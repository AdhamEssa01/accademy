namespace Academy.Application.Abstractions.Security;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid? UserId { get; }

    Guid? AcademyId { get; }

    IReadOnlyList<string> Roles { get; }

    string? Email { get; }
}