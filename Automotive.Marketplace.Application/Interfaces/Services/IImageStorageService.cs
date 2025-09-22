using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IImageStorageService
{
    public Task<string> UploadImageAsync(IFormFile file, string fileName);

    public Task<string> GetPresignedUrlAsync(string fileName);
}