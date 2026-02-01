using Academy.Application.Abstractions.Cms;
using Academy.Application.Contracts.Cms;
using Academy.Shared.Security;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/cms/pages")]
public sealed class CmsPagesController : ControllerBase
{
    private readonly ICmsService _cmsService;

    public CmsPagesController(ICmsService cmsService)
    {
        _cmsService = cmsService;
    }

    [HttpGet("{slug}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<CmsPageDto>> Get(string slug, CancellationToken ct)
    {
        var page = await _cmsService.GetPageAsync(slug, ct);
        return Ok(page);
    }

    [HttpPut("{slug}")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<CmsPageDto>> Update(
        string slug,
        [FromBody] UpdateCmsPageRequest request,
        CancellationToken ct)
    {
        var page = await _cmsService.UpdatePageAsync(slug, request, ct);
        return Ok(page);
    }

    [HttpPut("{slug}/sections")]
    [Authorize(Policy = Policies.Admin)]
    public async Task<ActionResult<IReadOnlyList<CmsSectionDto>>> UpdateSections(
        string slug,
        [FromBody] UpdateCmsSectionsRequest request,
        CancellationToken ct)
    {
        var sections = await _cmsService.UpdateSectionsAsync(slug, request, ct);
        return Ok(sections);
    }
}
