namespace Automotive.Marketplace.Server.Controllers
{
    using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;


    [Route("api/[controller]")]
    [ApiController]
    public class ListingController : ControllerBase
    {
        private readonly IMediator mediator;

        public ListingController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<GetListingDetailsWithCarResponse>> GetListingDetailsWithCar()
        {
            return await this.mediator.Send(new GetListingDetailsWithCarRequest());
        }

    }
}
