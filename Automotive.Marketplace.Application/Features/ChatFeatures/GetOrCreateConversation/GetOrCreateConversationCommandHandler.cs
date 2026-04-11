using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetOrCreateConversation;

public class GetOrCreateConversationCommandHandler(IRepository repository)
    : IRequestHandler<GetOrCreateConversationCommand, GetOrCreateConversationResponse>
{
    public async Task<GetOrCreateConversationResponse> Handle(
        GetOrCreateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);

        if (listing.SellerId == request.BuyerId)
        {
            throw new RequestValidationException(
            [
                new ValidationFailure("ListingId", "You cannot start a conversation about your own listing.")
            ]);
        }

        var existing = await repository.AsQueryable<Conversation>()
            .FirstOrDefaultAsync(
                c => c.BuyerId == request.BuyerId && c.ListingId == request.ListingId,
                cancellationToken);

        if (existing is not null)
            return new GetOrCreateConversationResponse { ConversationId = existing.Id };

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            BuyerId = request.BuyerId,
            ListingId = request.ListingId,
            CreatedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            CreatedBy = request.BuyerId.ToString()
        };

        await repository.CreateAsync(conversation, cancellationToken);
        return new GetOrCreateConversationResponse { ConversationId = conversation.Id };
    }
}
