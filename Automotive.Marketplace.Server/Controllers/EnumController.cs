using Automotive.Marketplace.Application.Features.EnumFeatures.GetBodyTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetDrivetrainTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetFuelTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetTransmissionTypes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class EnumController(IMediator mediator) : BaseController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetTransmissionTypesResponse>>> GetTransmissionTypes(CancellationToken cancellationToken)
    {
        var query = new GetTransmissionTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetFuelTypesResponse>>> GetFuelTypes(CancellationToken cancellationToken)
    {
        var query = new GetFuelTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetBodyTypesResponse>>> GetBodyTypes(CancellationToken cancellationToken)
    {
        var query = new GetBodyTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetDrivetrainTypesResponse>>> GetDrivetrainTypes(CancellationToken cancellationToken)
    {
        var query = new GetDrivetrainTypesQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
