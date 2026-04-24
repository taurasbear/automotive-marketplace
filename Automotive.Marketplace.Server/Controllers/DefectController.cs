using Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;
using Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;
using Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;
using Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;
using Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class DefectController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetDefectCategoriesResponse>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDefectCategoriesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult<Guid>> Add(
        [FromBody] AddListingDefectCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> Remove(
        [FromQuery] RemoveListingDefectCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult<Guid>> AddImage(
        [FromForm] AddDefectImageCommand command,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(command, cancellationToken);
        return Ok(id);
    }

    [HttpDelete]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> RemoveImage(
        [FromQuery] RemoveDefectImageCommand command,
        CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
