using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.CarFeatures.CreateCar;
using Automotive.Marketplace.Application.Features.CarFeatures.DeleteCar;
using Automotive.Marketplace.Application.Features.CarFeatures.GetAllCars;
using Automotive.Marketplace.Application.Features.CarFeatures.GetCarById;
using Automotive.Marketplace.Application.Features.CarFeatures.UpdateCar;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class CarController(IMediator mediator) : BaseController
{
    [HttpGet]
    [Protect(Permission.ViewCars)]
    public async Task<ActionResult<GetCarByIdResponse>> GetById([FromQuery] GetCarByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.ViewCars)]
    public async Task<ActionResult<GetAllCarsResponse>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllCarsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageCars, Permission.CreateCars)]
    public async Task<ActionResult> Create([FromBody] CreateCarCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Created();
    }

    [HttpDelete]
    [Protect(Permission.ManageCars)]
    public async Task<ActionResult> Delete([FromQuery] DeleteCarCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Protect(Permission.ManageCars)]
    public async Task<ActionResult> Update([FromBody] UpdateCarCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
