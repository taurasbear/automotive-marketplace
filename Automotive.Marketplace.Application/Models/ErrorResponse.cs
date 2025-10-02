namespace Automotive.Marketplace.Application.Models;

public class ErrorResponse
{
    public Dictionary<string, string[]>? Messages { get; set; }

    public string? Message { get; set; }

    public string? Type { get; set; }
}