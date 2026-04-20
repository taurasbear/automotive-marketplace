using Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class DrivetrainController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllDrivetrainsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDrivetrainsQuery(), cancellationToken);
        return Ok(result);
    }
}
