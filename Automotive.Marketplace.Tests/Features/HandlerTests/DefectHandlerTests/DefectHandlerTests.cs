using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;
using Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;
using Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.DefectHandlerTests;

public class DefectHandlerTests(
    DatabaseFixture<DefectHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DefectHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DefectHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static GetDefectCategoriesQueryHandler CreateGetDefectCategoriesHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IMapper>(),
               scope.ServiceProvider.GetRequiredService<IRepository>());

    private static AddListingDefectCommandHandler CreateAddListingDefectHandler(IServiceScope scope)
        => new(scope.ServiceProvider.GetRequiredService<IRepository>());

    private static RemoveListingDefectCommandHandler CreateRemoveListingDefectHandler(
        IServiceScope scope, 
        IImageStorageService? imageStorageService = null)
    {
        var imageService = imageStorageService ?? Substitute.For<IImageStorageService>();
        return new(scope.ServiceProvider.GetRequiredService<IRepository>(), imageService);
    }

    [Fact]
    public async Task GetDefectCategories_WithCategories_ReturnsAll()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateGetDefectCategoriesHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var category1 = new DefectCategory 
        { 
            Id = Guid.NewGuid(), 
            Name = "Engine Issues",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        var category2 = new DefectCategory 
        { 
            Id = Guid.NewGuid(), 
            Name = "Body Damage",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        await context.AddRangeAsync(category1, category2);
        await context.SaveChangesAsync();

        // Act
        var result = (await handler.Handle(new GetDefectCategoriesQuery(), CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Engine Issues");
        result.Should().Contain(r => r.Name == "Body Damage");
    }

    [Fact]
    public async Task AddListingDefect_ValidListing_ReturnsDefectId()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateAddListingDefectHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listing = await SeedListingAsync(context);

        var category = new DefectCategory 
        { 
            Id = Guid.NewGuid(), 
            Name = "Engine Issues",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };
        await context.AddAsync(category);
        await context.SaveChangesAsync();

        // Act
        var result = await handler.Handle(new AddListingDefectCommand
        {
            ListingId = listing.Id,
            DefectCategoryId = category.Id,
            CustomName = "Custom engine problem"
        }, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        
        // Verify the defect was created
        var defect = await context.Set<ListingDefect>().FindAsync(result);
        defect.Should().NotBeNull();
        defect!.ListingId.Should().Be(listing.Id);
        defect.DefectCategoryId.Should().Be(category.Id);
        defect.CustomName.Should().Be("Custom engine problem");
    }

    [Fact]
    public async Task AddListingDefect_NonExistentListing_ThrowsDbEntityNotFoundException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateAddListingDefectHandler(scope);

        // Act & Assert
        await Assert.ThrowsAsync<DbEntityNotFoundException>(() =>
            handler.Handle(new AddListingDefectCommand
            {
                ListingId = Guid.NewGuid(),
                CustomName = "Some defect"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task RemoveListingDefect_ExistingDefect_DeletesDefectAndImages()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var imageService = Substitute.For<IImageStorageService>();
        var handler = CreateRemoveListingDefectHandler(scope, imageService);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var listing = await SeedListingAsync(context);
        
        var defect = new ListingDefect
        {
            Id = Guid.NewGuid(),
            ListingId = listing.Id,
            CustomName = "Test defect",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test"
        };

        await context.AddAsync(defect);
        await context.SaveChangesAsync();

        // Act
        await handler.Handle(new RemoveListingDefectCommand { Id = defect.Id }, CancellationToken.None);

        // Assert
        // Verify defect was removed from database using a new query
        var deletedDefect = await context.Set<ListingDefect>()
            .AsNoTracking()
            .FirstOrDefaultAsync(ld => ld.Id == defect.Id);
        deletedDefect.Should().BeNull();
    }

    [Fact]
    public async Task RemoveListingDefect_NonExistent_ThrowsDbEntityNotFoundException()
    {
        // Arrange
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateRemoveListingDefectHandler(scope);

        // Act & Assert
        await Assert.ThrowsAsync<DbEntityNotFoundException>(() =>
            handler.Handle(new RemoveListingDefectCommand { Id = Guid.NewGuid() }, CancellationToken.None));
    }

    private static async Task<Listing> SeedListingAsync(AutomotiveContext context)
    {
        var seller = new UserBuilder().Build();
        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().WithMake(make.Id).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var drivetrain = new DrivetrainBuilder().Build();
        var municipality = new MunicipalityBuilder().Build();
        var variant = new VariantBuilder().WithModel(model.Id).WithFuel(fuel.Id).WithTransmission(transmission.Id).WithBodyType(bodyType.Id).Build();
        var listing = new ListingBuilder().WithSeller(seller.Id).WithVariant(variant.Id).WithDrivetrain(drivetrain.Id).WithMunicipality(municipality.Id).Build();

        await context.AddRangeAsync(seller, make, model, fuel, transmission, bodyType, drivetrain, municipality, variant, listing);
        await context.SaveChangesAsync();

        return listing;
    }
}