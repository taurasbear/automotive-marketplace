namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetListingDetailsWithCarHandler : BaseHandler<GetListingDetailsWithCarRequest, GetListingDetailsWithCarResponse>
    {
        public GetListingDetailsWithCarHandler(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        { }

        public override async Task<GetListingDetailsWithCarResponse> Handle(GetListingDetailsWithCarRequest request, CancellationToken cancellationToken)
        {
            var listingDetailsWithCar = await this.UnitOfWork.ListingRepository.GetListingDetailsWithCarAsync(cancellationToken);
            return this.Mapper.Map<GetListingDetailsWithCarResponse>(listingDetailsWithCar);
        }
    }
}
