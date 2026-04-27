using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public class GetUserContractProfileQueryHandler(IRepository repository)
    : IRequestHandler<GetUserContractProfileQuery, GetUserContractProfileResponse>
{
    public async Task<GetUserContractProfileResponse> Handle(
        GetUserContractProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync<User>(request.UserId, cancellationToken);

        return new GetUserContractProfileResponse
        {
            PhoneNumber = user.PhoneNumber,
            PersonalIdCode = user.PersonalIdCode,
            Address = user.Address,
        };
    }
}
