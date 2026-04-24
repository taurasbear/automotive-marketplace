using MediatR;
using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;

public sealed record AddDefectImageCommand : IRequest<Guid>
{
    public Guid ListingDefectId { get; set; }
    public IFormFile Image { get; set; } = null!;
}