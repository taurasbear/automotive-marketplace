namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;

public sealed record GetAllMakesResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = string.Empty;
}
