using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.DeleteModel;

public sealed record DeleteModelCommand : IRequest
{
    public Guid Id { get; set; }
};