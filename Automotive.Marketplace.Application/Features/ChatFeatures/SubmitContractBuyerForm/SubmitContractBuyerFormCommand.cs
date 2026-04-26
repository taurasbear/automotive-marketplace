using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.SubmitContractBuyerForm;

public sealed record SubmitContractBuyerFormCommand : IRequest<SubmitContractBuyerFormResponse>
{
    public Guid ContractCardId { get; set; }
    public Guid BuyerId { get; set; }

    public string PersonalIdCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    public bool UpdateProfile { get; set; }
}
