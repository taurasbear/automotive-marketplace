namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoryTranslationResponse
{
    public Guid Id { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
