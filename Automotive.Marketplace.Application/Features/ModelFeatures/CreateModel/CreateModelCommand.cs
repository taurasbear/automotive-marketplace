using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;

public sealed record CreateModelCommand : IRequest
{
    public string Name { get; set; } = string.Empty;

    public DateOnly FirstAppearanceDate { get; set; }

    public bool IsDiscontinued { get; set; }

    public Guid MakeId { get; set; }
};