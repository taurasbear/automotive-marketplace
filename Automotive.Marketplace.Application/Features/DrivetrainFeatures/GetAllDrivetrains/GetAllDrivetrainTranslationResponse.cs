namespace Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;

public sealed record GetAllDrivetrainTranslationResponse
{
    public Guid Id { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}
