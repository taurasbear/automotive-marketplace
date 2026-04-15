using Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class TransmissionController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllTransmissionsResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllTransmissionsQuery(), cancellationToken);
        return Ok(result);
    }
}
