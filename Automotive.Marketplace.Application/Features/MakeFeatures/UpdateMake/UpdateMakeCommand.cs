using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;

public sealed record UpdateMakeCommand : IRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
