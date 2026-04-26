using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;
using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class UserPreferencesController(IMediator mediator) : BaseController
{
    [HttpGet]
    [Protect(Permission.ViewListings)]
    public async Task<ActionResult<GetUserPreferencesResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserPreferencesQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Protect(Permission.ViewListings)]
    public async Task<ActionResult> Upsert(
        [FromBody] UpsertUserPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
