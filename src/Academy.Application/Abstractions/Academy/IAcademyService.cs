using Academy.Application.Contracts.Academies;

namespace Academy.Application.Abstractions.Academy;

public interface IAcademyService
{
    Task<AcademyDto> GetMyAcademyAsync(CancellationToken ct);

    Task<AcademyDto> UpdateMyAcademyAsync(UpdateAcademyRequest request, CancellationToken ct);
}
