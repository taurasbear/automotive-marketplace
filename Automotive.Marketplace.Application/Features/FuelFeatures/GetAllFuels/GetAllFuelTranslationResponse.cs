namespace Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;

public sealed record GetAllFuelTranslationResponse
{
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
