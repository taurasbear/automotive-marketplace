using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;

public sealed record UpdateUserContractProfileCommand : IRequest
{
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PersonalIdCode { get; set; }
    public string? Address { get; set; }
}
