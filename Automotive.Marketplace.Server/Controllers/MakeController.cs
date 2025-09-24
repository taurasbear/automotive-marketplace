using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class MakeController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMakesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllMakesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
