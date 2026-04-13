namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;

public sealed record GetAllModelsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

}