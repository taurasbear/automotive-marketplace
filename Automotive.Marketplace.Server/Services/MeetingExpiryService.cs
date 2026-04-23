using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Server.Services;

public class MeetingExpiryService(
    IServiceScopeFactory scopeFactory,
    IHubContext<ChatHub> hubContext,
    ILogger<MeetingExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            await ExpireMeetingsAsync(stoppingToken);
            await ExpireAvailabilityCardsAsync(stoppingToken);
        }
    }

    private async Task ExpireMeetingsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredMeetings = await repository.AsQueryable<Meeting>()
                .Where(m => m.Status == MeetingStatus.Pending && m.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var meeting in expiredMeetings)
            {
                meeting.Status = MeetingStatus.Expired;
                await repository.UpdateAsync(meeting, cancellationToken);

                var conversation = meeting.Conversation;
                var recipientId = meeting.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    meetingId = meeting.Id,
                    conversationId = meeting.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{meeting.InitiatorId}")
                    .SendAsync("MeetingExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("MeetingExpired", payload, cancellationToken);
            }

            if (expiredMeetings.Count > 0)
                logger.LogInformation("Expired {Count} pending meetings.", expiredMeetings.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring meetings.");
        }
    }

    private async Task ExpireAvailabilityCardsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredCards = await repository.AsQueryable<AvailabilityCard>()
                .Where(a => a.Status == AvailabilityCardStatus.Pending && a.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var card in expiredCards)
            {
                card.Status = AvailabilityCardStatus.Expired;
                await repository.UpdateAsync(card, cancellationToken);

                var conversation = card.Conversation;
                var recipientId = card.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    availabilityCardId = card.Id,
                    conversationId = card.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{card.InitiatorId}")
                    .SendAsync("AvailabilityExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("AvailabilityExpired", payload, cancellationToken);
            }

            if (expiredCards.Count > 0)
                logger.LogInformation("Expired {Count} pending availability cards.", expiredCards.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring availability cards.");
        }
    }
}