namespace Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;

public sealed record GetAllTransmissionTranslationResponse
{
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
