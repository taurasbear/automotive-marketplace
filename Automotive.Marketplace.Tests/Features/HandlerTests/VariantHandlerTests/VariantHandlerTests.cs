using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.DeleteVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;
using Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VariantHandlerTests;

public class VariantHandlerTests(DatabaseFixture<VariantHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<VariantHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<VariantHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    // === CreateVariant ===

    [Fact]
    public async Task CreateVariant_ValidCommand_CreatesSuccessfully()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        await context.AddRangeAsync(model, fuel, transmission, bodyType);
        await context.SaveChangesAsync();

        var handler = new CreateVariantCommandHandler(repository, mapper);

        var command = new CreateVariantCommand(
            ModelId: model.Id,
            FuelId: fuel.Id,
            TransmissionId: transmission.Id,
            BodyTypeId: bodyType.Id,
            IsCustom: false,
            DoorCount: 4,
            PowerKw: 100,
            EngineSizeMl: 2000
        );
        await handler.Handle(command, CancellationToken.None);

        var created = await context.Set<Variant>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.ModelId == model.Id && v.FuelId == fuel.Id);
        created.Should().NotBeNull();
        created!.ModelId.Should().Be(model.Id);
        created.FuelId.Should().Be(fuel.Id);
        created.PowerKw.Should().Be(100);
    }

    // === DeleteVariant ===

    [Fact]
    public async Task DeleteVariant_ExistingVariant_DeletesSuccessfully()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var variant = new VariantBuilder()
            .WithModel(model.Id)
            .WithFuel(fuel.Id)
            .WithTransmission(transmission.Id)
            .WithBodyType(bodyType.Id)
            .Build();
        await context.AddRangeAsync(model, fuel, transmission, bodyType, variant);
        await context.SaveChangesAsync();
        var variantId = variant.Id;

        var handler = new DeleteVariantCommandHandler(repository);

        await handler.Handle(new DeleteVariantCommand(variantId), CancellationToken.None);

        var deleted = await context.Set<Variant>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == variantId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteVariant_NonExistent_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var handler = new DeleteVariantCommandHandler(repository);

        var act = () => handler.Handle(
            new DeleteVariantCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    // === GetVariantsByModel ===

    [Fact]
    public async Task GetVariantsByModel_WithVariants_ReturnsFiltered()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model1 = new ModelBuilder().With(m => m.Make, make).Build();
        var model2 = new ModelBuilder().With(m => m.Make, make).Build();
        var fuel1 = new FuelBuilder().Build();
        var transmission1 = new TransmissionBuilder().Build();
        var bodyType1 = new BodyTypeBuilder().Build();
        var fuel2 = new FuelBuilder().Build();
        var transmission2 = new TransmissionBuilder().Build();
        var bodyType2 = new BodyTypeBuilder().Build();

        var variant1 = new VariantBuilder()
            .WithModel(model1.Id)
            .WithFuel(fuel1.Id)
            .WithTransmission(transmission1.Id)
            .WithBodyType(bodyType1.Id)
            .Build();
        var variant2 = new VariantBuilder()
            .WithModel(model2.Id)
            .WithFuel(fuel2.Id)
            .WithTransmission(transmission2.Id)
            .WithBodyType(bodyType2.Id)
            .Build();

        await context.AddRangeAsync(model1, model2, fuel1, transmission1, bodyType1, fuel2, transmission2, bodyType2, variant1, variant2);
        await context.SaveChangesAsync();

        var handler = new GetVariantsByModelQueryHandler(repository, mapper);

        var result = await handler.Handle(
            new GetVariantsByModelQuery(model1.Id),
            CancellationToken.None);

        result.Should().HaveCount(1);
    }

    // === UpdateVariant ===

    [Fact]
    public async Task UpdateVariant_ValidCommand_UpdatesProperties()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        var fuel = new FuelBuilder().Build();
        var transmission = new TransmissionBuilder().Build();
        var bodyType = new BodyTypeBuilder().Build();
        var variant = new VariantBuilder()
            .With(v => v.Model, model)
            .With(v => v.Fuel, fuel)
            .With(v => v.Transmission, transmission)
            .With(v => v.BodyType, bodyType)
            .Build();
        await context.AddRangeAsync(model, fuel, transmission, bodyType, variant);
        await context.SaveChangesAsync();

        var newFuel = new FuelBuilder().Build();
        var newTransmission = new TransmissionBuilder().Build();
        var newBodyType = new BodyTypeBuilder().Build();
        await context.AddRangeAsync(newFuel, newTransmission, newBodyType);
        await context.SaveChangesAsync();

        var handler = new UpdateVariantCommandHandler(repository, mapper);

        var command = new UpdateVariantCommand(
            Id: variant.Id,
            ModelId: model.Id,
            FuelId: newFuel.Id,
            TransmissionId: newTransmission.Id,
            BodyTypeId: newBodyType.Id,
            IsCustom: true,
            DoorCount: 2,
            PowerKw: 200,
            EngineSizeMl: 3000
        );

        await handler.Handle(command, CancellationToken.None);

        var updated = await context.Set<Variant>().AsNoTracking()
            .FirstAsync(v => v.Id == variant.Id);
        updated.FuelId.Should().Be(newFuel.Id);
        updated.PowerKw.Should().Be(200);
        updated.IsCustom.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateVariant_NonExistent_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var handler = new UpdateVariantCommandHandler(repository, mapper);

        var act = () => handler.Handle(
            new UpdateVariantCommand(
                Id: Guid.NewGuid(),
                ModelId: Guid.NewGuid(),
                FuelId: Guid.NewGuid(),
                TransmissionId: Guid.NewGuid(),
                BodyTypeId: Guid.NewGuid(),
                IsCustom: false,
                DoorCount: 4,
                PowerKw: 100,
                EngineSizeMl: 2000
            ),
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
