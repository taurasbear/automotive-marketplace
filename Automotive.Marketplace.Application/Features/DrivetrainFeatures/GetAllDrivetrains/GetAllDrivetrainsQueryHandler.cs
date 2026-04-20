using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;

public class GetAllDrivetrainsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllDrivetrainsQuery, IEnumerable<GetAllDrivetrainsResponse>>
{
    public async Task<IEnumerable<GetAllDrivetrainsResponse>> Handle(GetAllDrivetrainsQuery request, CancellationToken cancellationToken)
    {
        var drivetrains = await repository
            .AsQueryable<Drivetrain>()
            .Include(d => d.Translations)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetAllDrivetrainsResponse>>(drivetrains);
    }
}
