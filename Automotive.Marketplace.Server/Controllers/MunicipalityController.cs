using Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class MunicipalityController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetAllMunicipalitiesResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllMunicipalitiesQuery(), cancellationToken);
        return Ok(result);
    }
}
