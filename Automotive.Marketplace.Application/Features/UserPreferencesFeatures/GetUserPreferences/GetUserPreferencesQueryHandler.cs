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
                ValueWeight = 0.26,
                EfficiencyWeight = 0.21,
                ReliabilityWeight = 0.21,
                MileageWeight = 0.17,
                ConditionWeight = 0.15,
                AutoGenerateAiSummary = true,
                EnableVehicleScoring = false,
                HasCompletedQuiz = false,
                HasPreferences = false,
            };
        }

        return new GetUserPreferencesResponse
        {
            ValueWeight = prefs.ValueWeight,
            EfficiencyWeight = prefs.EfficiencyWeight,
            ReliabilityWeight = prefs.ReliabilityWeight,
            MileageWeight = prefs.MileageWeight,
            ConditionWeight = prefs.ConditionWeight,
            AutoGenerateAiSummary = prefs.AutoGenerateAiSummary,
            EnableVehicleScoring = prefs.EnableVehicleScoring,
            HasCompletedQuiz = prefs.HasCompletedQuiz,
            HasPreferences = true,
        };
    }
}
