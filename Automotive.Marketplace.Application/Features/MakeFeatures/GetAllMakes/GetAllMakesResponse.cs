namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;

public sealed record GetAllMakesResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
