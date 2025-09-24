namespace Automotive.Marketplace.Domain.Entities;

public class Image : BaseEntity
{
    public string BucketName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string OriginalFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string AltText { get; set; } = string.Empty;

    public Guid ListingId { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
