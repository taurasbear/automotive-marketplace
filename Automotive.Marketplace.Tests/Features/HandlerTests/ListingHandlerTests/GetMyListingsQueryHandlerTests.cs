using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ListingHandlerTests;

public class GetMyListingsQueryHandlerTests(
    DatabaseFixture<GetMyListingsQueryHandlerTests> fixture)
        : IClassFixture<DatabaseFixture<GetMyListingsQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetMyListingsQueryHandlerTests> _fixture = fixture;
    private readonly IImageStorageService _imageStorageService = Substitute.For<IImageStorageService>();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetMyListingsQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetMyListingsQueryHandler(mapper, repository, _imageStorageService);
    }

    private async Task<(Listing listing, Domain.Entities.User seller)> SeedListingAsync(AutomotiveContext context)
    {
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id)
            .Build();
        var seller = new UserBuilder().Build();
        var listing = new ListingBuilder()
            .WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id)
            .Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, listing);
        await context.SaveChangesAsync();
        return (listing, seller);
    }

    [Fact]
    public async Task Handle_NoListings_ShouldReturnEmptyList()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var query = new GetMyListingsQuery { SellerId = Guid.NewGuid() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_OnlyReturnsSellersOwnListings()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listing1, seller1) = await SeedListingAsync(context);
        _ = await SeedListingAsync(context);

        var query = new GetMyListingsQuery { SellerId = seller1.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Id.Should().Be(listing1.Id);
    }

    [Fact]
    public async Task Handle_ListingWithImages_ShouldPopulateImagesAndThumbnail()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var (listing, seller) = await SeedListingAsync(context);

        var images = new List<Image>
        {
            new Image { Id = Guid.NewGuid(), ObjectKey = "test-key-1", AltText = "test 1", ListingId = listing.Id, ListingDefectId = null, CreatedAt = DateTime.UtcNow, CreatedBy = "test" },
            new Image { Id = Guid.NewGuid(), ObjectKey = "test-key-2", AltText = "test 2", ListingId = listing.Id, ListingDefectId = null, CreatedAt = DateTime.UtcNow, CreatedBy = "test" }
        };
        await context.AddRangeAsync(images);
        await context.SaveChangesAsync();

        _imageStorageService.GetPresignedUrlAsync(Arg.Any<string>()).Returns("https://test-url");

        var query = new GetMyListingsQuery { SellerId = seller.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Images.Count().Should().Be(2);
        result.Single().Thumbnail.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ListingWithLikes_ShouldPopulateLikeCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var user1 = new UserBuilder().Build();
        var user2 = new UserBuilder().Build();
        var user3 = new UserBuilder().Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var likes = new[]
        {
            new UserListingLikeBuilder().WithListing(listing.Id).WithUser(user1.Id).Build(),
            new UserListingLikeBuilder().WithListing(listing.Id).WithUser(user2.Id).Build(),
            new UserListingLikeBuilder().WithListing(listing.Id).WithUser(user3.Id).Build()
        };
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, user1, user2, user3, listing);
        await context.AddRangeAsync(likes);
        await context.SaveChangesAsync();

        var query = new GetMyListingsQuery { SellerId = seller.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().LikeCount.Should().Be(3);
    }

    [Fact]
    public async Task Handle_ListingWithConversations_ShouldPopulateConversationCount()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var buyer1 = new UserBuilder().Build();
        var buyer2 = new UserBuilder().Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        var conv1 = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer1.Id).Build();
        var conv2 = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer2.Id).Build();
        var msg1 = new MessageBuilder().WithConversation(conv1.Id).WithSender(buyer1.Id).Build();
        var msg2 = new MessageBuilder().WithConversation(conv2.Id).WithSender(buyer2.Id).Build();
        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, buyer1, buyer2, listing);
        await context.AddRangeAsync(conv1, conv2, msg1, msg2);
        await context.SaveChangesAsync();

        var query = new GetMyListingsQuery { SellerId = seller.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().ConversationCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ListingWithMixedConversations_ShouldCountOnlyConversationsWithMessages()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var seller = new UserBuilder().Build();
        var buyer1 = new UserBuilder().Build();
        var buyer2 = new UserBuilder().Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).Build();
        // conversation with a message
        var conversationWithMessage = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer1.Id).Build();
        var message = new MessageBuilder().WithConversation(conversationWithMessage.Id).WithSender(buyer1.Id).Build();
        // conversation with no messages (buyer opened chat but sent nothing)
        var emptyConversation = new ConversationBuilder().WithListing(listing.Id).WithBuyer(buyer2.Id).Build();

        await context.AddRangeAsync(make, model, fuel, transmission, bodyType, drivetrain, variant, seller, buyer1, buyer2, listing);
        await context.AddRangeAsync(conversationWithMessage, message, emptyConversation);
        await context.SaveChangesAsync();

        var query = new GetMyListingsQuery { SellerId = seller.Id };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Single().ConversationCount.Should().Be(1);
    }
}
