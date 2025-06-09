namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using System.Threading;
using System.Threading.Tasks;

public class GetListingDetailsWithCarHandler(
    IMapper mapper,
    IUnitOfWork unitOfWork) : BaseHandler<GetListingDetailsWithCarRequest, GetListingsDetailsWithCarResponse>(mapper, unitOfWork)
{
    public override async Task<GetListingsDetailsWithCarResponse> Handle(
        GetListingDetailsWithCarRequest request,
        CancellationToken cancellationToken)
    {
        var listingDetailsWithCar = await this.UnitOfWork.ListingRepository.GetListingDetailsWithCarAsync(cancellationToken);
        return this.Mapper.Map<GetListingsDetailsWithCarResponse>(listingDetailsWithCar);
    }
}
