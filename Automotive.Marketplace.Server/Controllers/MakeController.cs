using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
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

    [HttpGet]
    [Protect(Permission.ViewMakes)]
    public async Task<ActionResult<GetMakeByIdResponse>> GetById([FromQuery] GetMakeByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageMakes, Permission.CreateMakes)]
    public async Task<ActionResult> Create([FromBody] CreateMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Created();
    }

    [HttpPut]
    [Protect(Permission.ManageMakes)]
    public async Task<ActionResult> Update([FromBody] UpdateMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    [Protect(Permission.ManageMakes)]
    public async Task<ActionResult> Delete([FromQuery] DeleteMakeCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
