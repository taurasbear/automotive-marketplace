namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetDrivetrainTypes;

public sealed record GetDrivetrainTypesResponse
{
    public string DrivetrainType { get; set; } = string.Empty;
}
