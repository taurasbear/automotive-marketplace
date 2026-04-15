namespace Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;

public sealed record GetAllDrivetrainsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IEnumerable<GetAllDrivetrainTranslationResponse> Translations { get; set; } = [];
}
