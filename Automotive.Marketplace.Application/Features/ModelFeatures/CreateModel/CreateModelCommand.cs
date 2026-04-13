using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;

public sealed record CreateModelCommand : IRequest
{
    public string Name { get; set; } = string.Empty;

    public Guid MakeId { get; set; }
};