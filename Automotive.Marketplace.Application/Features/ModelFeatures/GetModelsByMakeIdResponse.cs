namespace Automotive.Marketplace.Application.Features.ModelFeatures;

public sealed record GetModelsByMakeIdResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}