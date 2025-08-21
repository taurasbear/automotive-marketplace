using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[Route("[controller]")]
[ApiController]
public class MakeController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMakesResponse>>> GetAll()
    {
        var result = await mediator.Send(new GetAllMakesQuery());
        return Ok(result);
    }
}
