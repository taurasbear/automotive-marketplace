using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;
using Automotive.Marketplace.Application.Features.ModelFeatures.DeleteModel;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;
using Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;
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

namespace Automotive.Marketplace.Tests.Features.HandlerTests.ModelHandlerTests;

public class ModelHandlerTests(DatabaseFixture<ModelHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<ModelHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<ModelHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    // === CreateModel ===

    [Fact]
    public async Task CreateModel_ValidCommand_CreatesSuccessfully()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var handler = new CreateModelCommandHandler(mapper, repository);

        var command = new CreateModelCommand { Name = "TestModel", MakeId = make.Id };
        await handler.Handle(command, CancellationToken.None);

        var created = await context.Set<Model>().AsNoTracking()
            .FirstOrDefaultAsync(m => m.Name == "TestModel");
        created.Should().NotBeNull();
        created!.MakeId.Should().Be(make.Id);
    }

    // === DeleteModel ===

    [Fact]
    public async Task DeleteModel_ExistingModel_DeletesSuccessfully()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        await context.AddAsync(model);
        await context.SaveChangesAsync();
        var modelId = model.Id;

        var handler = new DeleteModelCommandHandler(repository);

        await handler.Handle(new DeleteModelCommand { Id = modelId }, CancellationToken.None);

        var deleted = await context.Set<Model>().AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == modelId);
        deleted.Should().BeNull();
    }

    [Fact]
    public async Task DeleteModel_NonExistent_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        var handler = new DeleteModelCommandHandler(repository);

        var act = () => handler.Handle(
            new DeleteModelCommand { Id = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    // === GetAllModels ===

    [Fact]
    public async Task GetAllModels_WithData_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model1 = new ModelBuilder().With(m => m.Make, make).Build();
        var model2 = new ModelBuilder().With(m => m.Make, make).Build();
        await context.AddRangeAsync(model1, model2);
        await context.SaveChangesAsync();

        var handler = new GetAllModelsQueryHandler(mapper, repository);

        var result = await handler.Handle(new GetAllModelsQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // === GetModelById ===

    [Fact]
    public async Task GetModelById_Existing_ReturnsModel()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        await context.AddAsync(model);
        await context.SaveChangesAsync();

        var handler = new GetModelByIdQueryHandler(mapper, repository);

        var result = await handler.Handle(
            new GetModelByIdQuery { Id = model.Id },
            CancellationToken.None);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetModelById_NonExistent_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var handler = new GetModelByIdQueryHandler(mapper, repository);

        var act = () => handler.Handle(
            new GetModelByIdQuery { Id = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }

    // === GetModelsByMakeId ===

    [Fact]
    public async Task GetModelsByMakeId_WithModels_ReturnsFiltered()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make1 = new MakeBuilder().Build();
        var make2 = new MakeBuilder().Build();
        var model1 = new ModelBuilder().With(m => m.Make, make1).Build();
        var model2 = new ModelBuilder().With(m => m.Make, make2).Build();
        await context.AddRangeAsync(model1, model2);
        await context.SaveChangesAsync();

        var handler = new GetModelsByMakeIdQueryHandler(mapper, repository);

        var result = await handler.Handle(
            new GetModelsByMakeIdQuery { MakeId = make1.Id },
            CancellationToken.None);

        result.Should().HaveCount(1);
    }

    // === UpdateModel ===

    [Fact]
    public async Task UpdateModel_ValidCommand_UpdatesName()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var make = new MakeBuilder().Build();
        var model = new ModelBuilder().With(m => m.Make, make).Build();
        await context.AddAsync(model);
        await context.SaveChangesAsync();

        var handler = new UpdateModelCommandHandler(repository, mapper);

        var command = new UpdateModelCommand
        {
            Id = model.Id,
            Name = "UpdatedName",
            MakeId = make.Id,
        };

        await handler.Handle(command, CancellationToken.None);

        var updated = await context.Set<Model>().AsNoTracking()
            .FirstAsync(m => m.Id == model.Id);
        updated.Name.Should().Be("UpdatedName");
    }

    [Fact]
    public async Task UpdateModel_NonExistent_ThrowsDbEntityNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var handler = new UpdateModelCommandHandler(repository, mapper);

        var act = () => handler.Handle(
            new UpdateModelCommand { Id = Guid.NewGuid(), Name = "X", MakeId = Guid.NewGuid() },
            CancellationToken.None);

        await act.Should().ThrowAsync<DbEntityNotFoundException>();
    }
}
