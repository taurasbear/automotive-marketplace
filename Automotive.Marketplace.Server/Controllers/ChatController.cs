using Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversationAsSeller;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetUnreadCount;
using Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetContractCard;
using Automotive.Marketplace.Application.Features.ChatFeatures.ExportContractPdf;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;
using Automotive.Marketplace.Application.Features.ChatFeatures.UpdateUserContractProfile;
using Automotive.Marketplace.Server.Hubs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Automotive.Marketplace.Server.Controllers;

[Authorize]
public class ChatController(IMediator mediator, IHubContext<ChatHub> hubContext) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<GetOrCreateConversationResponse>> GetOrCreateConversation(
        [FromBody] GetOrCreateConversationCommand command,
        CancellationToken cancellationToken)
    {
        command.BuyerId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GetOrCreateConversationAsSellerResponse>> GetOrCreateConversationAsSeller(
        [FromBody] GetOrCreateConversationAsSellerCommand command,
        CancellationToken cancellationToken)
    {
        command.SellerId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationSummaryResponse>>> GetConversations(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetConversationsQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetMessagesResponse>> GetMessages(
        [FromQuery] GetMessagesQuery query,
        CancellationToken cancellationToken)
    {
        query.UserId = UserId;
        var result = await mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> MarkMessagesRead(
        [FromBody] MarkMessagesReadCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        var result = await mediator.Send(command, cancellationToken);
        await hubContext.Clients
            .Group($"user-{UserId}")
            .SendAsync("UpdateUnreadCount", result.TotalUnreadCount, cancellationToken);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<GetUnreadCountResponse>> GetUnreadCount(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetUnreadCountQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<GetContractCardResponse>> GetContractCard(
        [FromQuery] Guid contractCardId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetContractCardQuery { ContractCardId = contractCardId, RequesterId = UserId },
            cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> ExportContractPdf(
        [FromQuery] Guid contractCardId,
        CancellationToken cancellationToken)
    {
        var bytes = await mediator.Send(
            new ExportContractPdfQuery { ContractCardId = contractCardId, RequesterId = UserId },
            cancellationToken);
        return File(bytes, "application/pdf", "contract.pdf");
    }

    [HttpGet]
    public async Task<ActionResult<GetUserContractProfileResponse>> GetUserContractProfile(
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetUserContractProfileQuery { UserId = UserId }, cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUserContractProfile(
        [FromBody] UpdateUserContractProfileCommand command,
        CancellationToken cancellationToken)
    {
        command.UserId = UserId;
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
