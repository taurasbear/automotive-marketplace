namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;

public sealed record GetModelByIdResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid MakeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string ModifiedBy { get; set; } = string.Empty;
}