using MediatR;

namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IRequest<GetDashboardSummaryResponse>
{
    public Guid CurrentUserId { get; set; }
}