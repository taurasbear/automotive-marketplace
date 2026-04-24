using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;

public sealed record RemoveDefectImageCommand : IRequest
{
    public Guid Id { get; set; }
}