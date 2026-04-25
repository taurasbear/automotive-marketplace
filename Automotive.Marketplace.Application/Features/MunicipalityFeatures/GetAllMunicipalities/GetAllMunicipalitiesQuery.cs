using MediatR;

namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public sealed record GetAllMunicipalitiesQuery : IRequest<IEnumerable<GetAllMunicipalitiesResponse>>;
