using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;

public sealed record AddListingDefectCommand : IRequest<Guid>
{
    public Guid ListingId { get; set; }
    public Guid? DefectCategoryId { get; set; }
    public string? CustomName { get; set; }
}