using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;

public sealed record RemoveListingDefectCommand : IRequest
{
    public Guid Id { get; set; }
}