using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllListingsResponse>>> GetById(
       [FromQuery] GetListingByIdQuery query,
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

    [HttpDelete]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> Delete([FromQuery] DeleteListingCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult> Update([FromBody] UpdateListingCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
