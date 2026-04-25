namespace Automotive.Marketplace.Application.Models;

public sealed class ListingDefectDto
{
    public Guid Id { get; set; }
    public Guid? DefectCategoryId { get; set; }
    public string? DefectCategoryName { get; set; }
    public string? CustomName { get; set; }
    public IEnumerable<ImageDto> Images { get; set; } = [];
}