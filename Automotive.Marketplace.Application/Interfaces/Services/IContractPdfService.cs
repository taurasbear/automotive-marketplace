using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IContractPdfService
{
    byte[] Generate(ContractCard card, ContractSellerSubmission seller, ContractBuyerSubmission buyer);
}
