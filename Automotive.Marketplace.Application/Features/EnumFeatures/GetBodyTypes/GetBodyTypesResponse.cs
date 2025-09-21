namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetBodyTypes;

public sealed record GetBodyTypesResponse
{
    public string BodyType { get; set; } = string.Empty;
}
