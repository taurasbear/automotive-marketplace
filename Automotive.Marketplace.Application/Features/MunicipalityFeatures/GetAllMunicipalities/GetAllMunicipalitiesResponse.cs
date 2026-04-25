namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public sealed record GetAllMunicipalitiesResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
