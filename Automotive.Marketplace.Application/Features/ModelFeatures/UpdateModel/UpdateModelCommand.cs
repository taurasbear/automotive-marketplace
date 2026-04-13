using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;

public sealed record UpdateModelCommand : IRequest
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid MakeId { get; set; }
};