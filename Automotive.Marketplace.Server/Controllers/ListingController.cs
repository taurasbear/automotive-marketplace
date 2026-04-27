using Automotive.Marketplace.Application.Common.Models;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.DeleteListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingAiSummary;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparisonAiSummary;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetSellerListingInsights;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class ListingController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<GetAllListingsResponse>>> GetAll(
        [FromQuery] GetAllListingsQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId != Guid.Empty ? UserId : null;
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
    public async Task<ActionResult<CreateListingResponse>> Create(
        [FromForm] CreateListingCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command with { SellerId = UserId }, cancellationToken);
        return Created("", result);
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SearchListingsResponse>>> Search(
        [FromQuery] SearchListingsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetListingComparisonResponse>> Compare(
        [FromQuery] Guid a,
        [FromQuery] Guid b,
        CancellationToken cancellationToken)
    {
        var query = new GetListingComparisonQuery { ListingAId = a, ListingBId = b };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.CreateListings, Permission.ManageListings)]
    public async Task<ActionResult<IEnumerable<GetMyListingsResponse>>> GetMy(
        CancellationToken cancellationToken)
    {
        var query = new GetMyListingsQuery { SellerId = UserId };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.CreateListings, Permission.ManageListings)]
    public async Task<ActionResult<GetListingEngagementsResponse>> GetEngagements(
        [FromQuery] Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetListingEngagementsQuery { ListingId = id, CurrentUserId = UserId };
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetListingScoreResponse>> GetScore(
        [FromQuery] GetListingScoreQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId != Guid.Empty ? UserId : null;
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetListingAiSummaryResponse>> GetAiSummary(
        [FromQuery] GetListingAiSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetListingComparisonAiSummaryResponse>> GetComparisonAiSummary(
        [FromQuery] GetListingComparisonAiSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.ManageListings)]
    public async Task<ActionResult<GetSellerListingInsightsResponse>> GetSellerInsights(
        [FromQuery] GetSellerListingInsightsQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId;
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
