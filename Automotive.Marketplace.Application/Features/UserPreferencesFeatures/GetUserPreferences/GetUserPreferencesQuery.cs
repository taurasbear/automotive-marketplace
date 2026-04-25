using MediatR;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesQuery : IRequest<GetUserPreferencesResponse>
{
    public Guid UserId { get; set; }
}
