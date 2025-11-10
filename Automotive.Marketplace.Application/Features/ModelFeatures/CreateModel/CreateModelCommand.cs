using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;

public sealed record CreateModelCommand : IRequest
{
    public required string Name { get; set; } = string.Empty;

    public required DateOnly FirstAppearanceDate { get; set; }

    public required bool IsDiscontinued { get; set; }

    public required Guid MakeId { get; set; }
};