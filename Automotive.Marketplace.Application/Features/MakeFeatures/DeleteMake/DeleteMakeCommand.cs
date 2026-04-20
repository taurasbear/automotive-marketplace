using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;

public sealed record DeleteMakeCommand : IRequest
{
    public Guid Id { get; set; }
}
