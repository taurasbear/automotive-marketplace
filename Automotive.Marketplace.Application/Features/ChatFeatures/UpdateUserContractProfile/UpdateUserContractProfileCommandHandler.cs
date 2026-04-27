using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;

public class UpdateUserContractProfileCommandHandler(IRepository repository)
    : IRequestHandler<UpdateUserContractProfileCommand>
{
    public async Task Handle(
        UpdateUserContractProfileCommand request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetByIdAsync<User>(request.UserId, cancellationToken);
        user.PhoneNumber = request.PhoneNumber;
        user.PersonalIdCode = request.PersonalIdCode;
        user.Address = request.Address;
        await repository.UpdateAsync(user, cancellationToken);
    }
}
