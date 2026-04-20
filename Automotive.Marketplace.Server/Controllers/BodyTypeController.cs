using Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class BodyTypeController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllBodyTypesResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllBodyTypesQuery(), cancellationToken);
        return Ok(result);
    }
}
