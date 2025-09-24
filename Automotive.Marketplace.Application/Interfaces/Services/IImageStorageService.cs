using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IImageStorageService
{
    public Task<ImageUploadResult> UploadImageAsync(IFormFile file, string fileName);

    public Task<string> GetPresignedUrlAsync(string fileName);
}