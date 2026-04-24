using Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.DeleteVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;
using Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class VariantController(IMediator mediator) : BaseController
{
    [HttpGet("{modelId:guid}")]
    public async Task<ActionResult<IEnumerable<GetVariantsByModelResponse>>> GetByModelId(
        Guid modelId, CancellationToken cancellationToken)
    {
        var query = new GetVariantsByModelQuery(modelId);
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Protect(Permission.CreateVariants, Permission.ManageVariants)]
    public async Task<ActionResult> Create([FromBody] CreateVariantCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Created("", result);
    }

    [HttpPut("{id:guid}")]
    [Protect(Permission.ManageVariants)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateVariantCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Protect(Permission.ManageVariants)]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteVariantCommand(id), cancellationToken);
        return NoContent();
    }
}
