namespace Automotive.Marketplace.Domain.Entities;

public class TransmissionTranslation : BaseEntity
{
    public Guid TransmissionId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual Transmission Transmission { get; set; } = null!;
}
