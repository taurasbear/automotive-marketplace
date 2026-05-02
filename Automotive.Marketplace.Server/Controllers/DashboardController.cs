using Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IMediator mediator) : BaseController
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDashboardSummaryQuery
        {
            CurrentUserId = UserId,
        }, cancellationToken);
        return Ok(result);
    }
}