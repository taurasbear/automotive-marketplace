namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public sealed record CreateVariantResponse
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }
    public int Year { get; set; }
    public Guid FuelId { get; set; }
    public Guid TransmissionId { get; set; }
    public Guid BodyTypeId { get; set; }
    public bool IsCustom { get; set; }
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
}
