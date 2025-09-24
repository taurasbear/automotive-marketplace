namespace Automotive.Marketplace.Application.Models;

public class ImageUploadResult
{
    public string ObjectKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public long FileSize { get; set; }
}