namespace Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;

public sealed record GetVariantsByModelResponse
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public Guid FuelId { get; set; }
    public string FuelName { get; set; } = string.Empty;
    public Guid TransmissionId { get; set; }
    public string TransmissionName { get; set; } = string.Empty;
    public Guid BodyTypeId { get; set; }
    public string BodyTypeName { get; set; } = string.Empty;
    public bool IsCustom { get; set; }
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
}
