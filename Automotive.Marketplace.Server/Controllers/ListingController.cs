namespace Automotive.Marketplace.Server.Controllers;

using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class ListingController(IMediator mediator) : ControllerBase
{
    private readonly IMediator mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<GetListingsDetailsWithCarResponse>> GetListingDetailsWithCar()
    {
        return await this.mediator.Send(new GetListingDetailsWithCarRequest());
    }
}
