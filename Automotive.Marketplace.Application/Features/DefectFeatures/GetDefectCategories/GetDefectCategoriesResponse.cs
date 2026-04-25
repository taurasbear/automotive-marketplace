namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoriesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IEnumerable<GetDefectCategoryTranslationResponse> Translations { get; set; } = [];
}
