using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class ModelController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMakesResponse>>> GetByMakeId(
        [FromQuery] GetModelsByMakeIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetModelByIdResponse>> GetById([FromQuery] GetModelByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
