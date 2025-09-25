namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public sealed record GetListingByIdResponse
{
    public string Year { get; set; } = string.Empty;

    public string Fuel { get; set; } = string.Empty;

    public string Transmission { get; set; } = string.Empty;

    public string BodyType { get; set; } = string.Empty;

    public string Drivetrain { get; set; } = string.Empty;

    public int DoorCount { get; set; }

    public Guid MakeId { get; set; }

    public Guid ModelId { get; set; }
}