namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetTransmissionTypes;

public sealed record GetTransmissionTypesResponse
{
    public string TransmissionType { get; set; } = string.Empty;
}
