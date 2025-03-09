namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Domain.Entities;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetListingDetailsWithCarHandler : BaseHandler<GetListingDetailsWithCarRequest, GetListingDetailsWithCarResponse>
    {
        public GetListingDetailsWithCarHandler(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        { }

        public override async Task<GetListingDetailsWithCarResponse> Handle(GetListingDetailsWithCarRequest request, CancellationToken cancellationToken)
        {
            IList<Listing> listingDetailsWithCar = await this.UnitOfWork.ListingRepository.GetListingDetailsWithCar(cancellationToken);
        }
    }
}
