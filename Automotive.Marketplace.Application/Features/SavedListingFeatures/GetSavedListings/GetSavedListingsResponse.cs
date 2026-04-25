using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.GetSavedListings;

public sealed record GetSavedListingsResponse
{
    public Guid ListingId { get; set; }

    public string Title { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string MunicipalityName { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public string FuelName { get; set; } = string.Empty;

    public string TransmissionName { get; set; } = string.Empty;

    public ImageDto? Thumbnail { get; set; }

    public string? NoteContent { get; set; }
}
