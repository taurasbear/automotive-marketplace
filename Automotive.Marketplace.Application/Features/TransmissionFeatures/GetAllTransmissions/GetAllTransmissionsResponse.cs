namespace Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;

public sealed record GetAllTransmissionsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IEnumerable<GetAllTransmissionTranslationResponse> Translations { get; set; } = [];
}
