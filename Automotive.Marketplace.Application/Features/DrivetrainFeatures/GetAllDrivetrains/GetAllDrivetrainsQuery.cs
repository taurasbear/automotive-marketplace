using MediatR;

namespace Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;

public sealed record GetAllDrivetrainsQuery : IRequest<IEnumerable<GetAllDrivetrainsResponse>>;
