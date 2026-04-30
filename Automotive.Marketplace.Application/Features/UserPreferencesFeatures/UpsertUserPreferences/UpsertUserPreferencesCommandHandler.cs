using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommandHandler(IRepository repository)
    : IRequestHandler<UpsertUserPreferencesCommand>
{
    public async Task Handle(UpsertUserPreferencesCommand request, CancellationToken cancellationToken)
    {
        var total = request.ValueWeight + request.EfficiencyWeight + request.ReliabilityWeight
                    + request.MileageWeight + request.ConditionWeight;
        if (Math.Abs(total - 1.0) > 0.01)
            throw new ArgumentException("Score weights must sum to 1.0");

        var existing = await repository.AsQueryable<UserPreferences>()
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);

        if (existing != null)
        {
            existing.ValueWeight = request.ValueWeight;
            existing.EfficiencyWeight = request.EfficiencyWeight;
            existing.ReliabilityWeight = request.ReliabilityWeight;
            existing.MileageWeight = request.MileageWeight;
            existing.ConditionWeight = request.ConditionWeight;
            existing.AutoGenerateAiSummary = request.AutoGenerateAiSummary;
            existing.EnableVehicleScoring = request.EnableVehicleScoring;
            existing.HasCompletedQuiz = request.HasCompletedQuiz;
            await repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await repository.CreateAsync(new UserPreferences
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ValueWeight = request.ValueWeight,
                EfficiencyWeight = request.EfficiencyWeight,
                ReliabilityWeight = request.ReliabilityWeight,
                MileageWeight = request.MileageWeight,
                ConditionWeight = request.ConditionWeight,
                AutoGenerateAiSummary = request.AutoGenerateAiSummary,
                EnableVehicleScoring = request.EnableVehicleScoring,
                HasCompletedQuiz = request.HasCompletedQuiz,
            }, cancellationToken);
        }
    }
}
