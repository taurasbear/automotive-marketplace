using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class ListingController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllListingsResponse>>> GetAll(
        [FromQuery] GetAllListingsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.CreateListings, Permission.ManageListings)]
    public async Task<ActionResult> Create(
        [FromForm] CreateListingCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return Created();
    }
}
