using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Server.Services;

public class OfferExpiryService(
    IServiceScopeFactory scopeFactory,
    IHubContext<ChatHub> hubContext,
    ILogger<OfferExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            await ExpireOffersAsync(stoppingToken);
        }
    }

    private async Task ExpireOffersAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

            var expiredOffers = await repository.AsQueryable<Offer>()
                .Where(o => o.Status == OfferStatus.Pending && o.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var offer in expiredOffers)
            {
                offer.Status = OfferStatus.Expired;
                await repository.UpdateAsync(offer, cancellationToken);

                var conversation = offer.Conversation;
                var recipientId = offer.InitiatorId == conversation.BuyerId
                    ? conversation.Listing.SellerId
                    : conversation.BuyerId;

                var payload = new
                {
                    offerId = offer.Id,
                    conversationId = offer.ConversationId
                };

                await hubContext.Clients
                    .Group($"user-{offer.InitiatorId}")
                    .SendAsync("OfferExpired", payload, cancellationToken);

                await hubContext.Clients
                    .Group($"user-{recipientId}")
                    .SendAsync("OfferExpired", payload, cancellationToken);
            }

            if (expiredOffers.Count > 0)
                logger.LogInformation("Expired {Count} pending offers.", expiredOffers.Count);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error while expiring offers.");
        }
    }
}
