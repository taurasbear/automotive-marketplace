using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;

public sealed record CreateMakeCommand : IRequest
{
    public string Name { get; set; } = string.Empty;
}
