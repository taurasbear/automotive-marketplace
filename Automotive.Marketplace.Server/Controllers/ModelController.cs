using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.ModelFeatures;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class ModelController(IMediator mediator) : BaseController
{
    [HttpGet]
    [Protect]
    public async Task<ActionResult<IEnumerable<GetAllMakesResponse>>> GetByMakeId(
        [FromQuery] GetModelsByMakeIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
