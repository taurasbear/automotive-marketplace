using Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;
using Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[Authorize]
public class SavedListingController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<ToggleLikeResponse>> ToggleLike(
        [FromBody] ToggleLikeCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetSavedListingsResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetSavedListingsQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpsertNote(
        [FromBody] UpsertListingNoteCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteNote(
        [FromQuery] DeleteListingNoteCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
