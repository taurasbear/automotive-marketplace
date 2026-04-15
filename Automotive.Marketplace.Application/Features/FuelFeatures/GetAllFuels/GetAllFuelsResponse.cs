namespace Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;

public sealed record GetAllFuelsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IEnumerable<GetAllFuelTranslationResponse> Translations { get; set; } = [];
}
