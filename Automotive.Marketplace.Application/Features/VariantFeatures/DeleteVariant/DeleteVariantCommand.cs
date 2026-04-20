using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.DeleteVariant;

public sealed record DeleteVariantCommand(Guid Id) : IRequest;
