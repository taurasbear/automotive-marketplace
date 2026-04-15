using Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class FuelController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllFuelsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllFuelsQuery(), cancellationToken);
        return Ok(result);
    }
}
