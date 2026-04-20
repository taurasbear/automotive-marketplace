namespace Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;

public sealed record GetAllBodyTypeTranslationResponse
{
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
