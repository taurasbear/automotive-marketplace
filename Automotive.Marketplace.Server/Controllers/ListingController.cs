namespace Automotive.Marketplace.Server.Controllers;

using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ListingController(IMediator mediator) : ControllerBase
{
    private readonly IMediator mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllListingsResponse>>> GetAll()
    {
        var result = await this.mediator.Send(new GetAllListingsQuery());
        return Ok(result);
    }
}
