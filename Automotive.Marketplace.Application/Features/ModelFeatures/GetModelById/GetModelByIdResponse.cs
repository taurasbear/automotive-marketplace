namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;

public sealed record GetModelByIdResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateOnly FirstAppearanceDate { get; set; }

    public bool IsDiscontinued { get; set; }
}