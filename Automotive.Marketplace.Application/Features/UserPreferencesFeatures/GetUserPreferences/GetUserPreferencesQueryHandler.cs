using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesQueryHandler(IRepository repository)
    : IRequestHandler<GetUserPreferencesQuery, GetUserPreferencesResponse>
{
    public async Task<GetUserPreferencesResponse> Handle(GetUserPreferencesQuery request, CancellationToken cancellationToken)
    {
        var prefs = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (prefs is null)
        {
            return new GetUserPreferencesResponse
            {
                ValueWeight = 0.30,
                EfficiencyWeight = 0.25,
                ReliabilityWeight = 0.25,
                MileageWeight = 0.20,
                AutoGenerateAiSummary = false,
                HasPreferences = false,
            };
        }

        return new GetUserPreferencesResponse
        {
            ValueWeight = prefs.ValueWeight,
            EfficiencyWeight = prefs.EfficiencyWeight,
            ReliabilityWeight = prefs.ReliabilityWeight,
            MileageWeight = prefs.MileageWeight,
            AutoGenerateAiSummary = prefs.AutoGenerateAiSummary,
            HasPreferences = true,
        };
    }
}
