namespace Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;

public sealed record GetAllBodyTypesResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IEnumerable<GetAllBodyTypeTranslationResponse> Translations { get; set; } = [];
}
