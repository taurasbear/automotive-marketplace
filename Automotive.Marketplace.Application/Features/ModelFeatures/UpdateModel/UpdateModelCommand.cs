using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;

public sealed record UpdateModelCommand : IRequest
{
    public required Guid Id { get; set; }

    public required string Name { get; set; } = string.Empty;

    public required DateOnly FirstAppearanceDate { get; set; }

    public required bool IsDiscontinued { get; set; }

    public required Guid MakeId { get; set; }
};