using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;
using Automotive.Marketplace.Application.Features.ModelFeatures.DeleteModel;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;
using Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
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
    [Protect(Permission.ViewModels)]
    public async Task<ActionResult<GetModelByIdResponse>> GetById([FromQuery] GetModelByIdQuery query, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [Protect(Permission.ViewModels)]
    public async Task<ActionResult<GetAllModelsResponse>> GetAll(CancellationToken cancellationToken)
    {
        var query = new GetAllModelsQuery();
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.ManageModels, Permission.CreateModels)]
    public async Task<ActionResult> Create([FromBody] CreateModelCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return Created();
    }

    [HttpDelete]
    [Protect(Permission.ManageModels)]
    public async Task<ActionResult> Delete([FromQuery] DeleteModelCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Protect(Permission.ManageModels)]
    public async Task<ActionResult> Update([FromBody] UpdateModelCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
