using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Automotive.Marketplace.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Automotive.Marketplace.Infrastructure.Services;

public class S3ImageStorageService(IAmazonS3 s3Client, IConfiguration configuration) : IImageStorageService
{
    private readonly string _bucketName = configuration["MinIO:BucketName"]!;

    public async Task<string> UploadImageAsync(IFormFile file, string fileName)
    {
        var safeFileName = WebUtility.HtmlEncode(Path.GetFileName(fileName));

        await using (var stream = file.OpenReadStream())
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = safeFileName,
                InputStream = stream,
                ContentType = file.ContentType,
                AutoCloseStream = true
            };

            await s3Client.PutObjectAsync(request);
        }

        return safeFileName ?? "";
    }

    public async Task<string> GetPresignedUrlAsync(string fileName)
    {
        var expires = DateTime.UtcNow
        .AddHours
        (
            Convert.ToDouble(configuration["MinIO:PresignedUrlExpirationHours"])
        );

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = fileName,
            Verb = HttpVerb.GET,
            Expires = expires,
            Protocol = Protocol.HTTP
        };

        var presignedURL = await s3Client.GetPreSignedURLAsync(request);

        return presignedURL;
    }
}