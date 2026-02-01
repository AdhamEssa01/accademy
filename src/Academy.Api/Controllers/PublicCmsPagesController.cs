using Academy.Application.Abstractions.Cms;
using Academy.Application.Contracts.Cms;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Academy.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/public/cms/pages")]
public sealed class PublicCmsPagesController : ControllerBase
{
    private readonly ICmsService _cmsService;

    public PublicCmsPagesController(ICmsService cmsService)
    {
        _cmsService = cmsService;
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<CmsPageDto>> Get(string slug, CancellationToken ct)
    {
        var page = await _cmsService.GetPublicPageAsync(slug, ct);
        if (page is null)
        {
            return NotFound();
        }

        return Ok(page);
    }
}
