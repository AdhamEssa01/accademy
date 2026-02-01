using Academy.Application.Contracts.Cms;

namespace Academy.Application.Abstractions.Cms;

public interface ICmsService
{
    Task<CmsPageDto> GetPageAsync(string slug, CancellationToken ct);

    Task<CmsPageDto> UpdatePageAsync(string slug, UpdateCmsPageRequest request, CancellationToken ct);

    Task<IReadOnlyList<CmsSectionDto>> UpdateSectionsAsync(string slug, UpdateCmsSectionsRequest request, CancellationToken ct);

    Task<CmsPageDto?> GetPublicPageAsync(string slug, CancellationToken ct);
}
