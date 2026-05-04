using Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;
using Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;
using Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;
using Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.LookupHandlerTests;

public class LookupHandlerTests(DatabaseFixture<LookupHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<LookupHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<LookupHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    // === GetAllBodyTypes ===

    [Fact]
    public async Task GetAllBodyTypes_WithData_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var bt1 = new BodyTypeBuilder().Build();
        var bt2 = new BodyTypeBuilder().Build();
        await context.AddRangeAsync(bt1, bt2);
        await context.SaveChangesAsync();

        var handler = new GetAllBodyTypesQueryHandler(mapper, repository);

        var result = await handler.Handle(new GetAllBodyTypesQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // === GetAllDrivetrains ===

    [Fact]
    public async Task GetAllDrivetrains_WithData_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var dt1 = new DrivetrainBuilder().Build();
        var dt2 = new DrivetrainBuilder().Build();
        await context.AddRangeAsync(dt1, dt2);
        await context.SaveChangesAsync();

        var handler = new GetAllDrivetrainsQueryHandler(mapper, repository);

        var result = await handler.Handle(new GetAllDrivetrainsQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // === GetAllFuels ===

    [Fact]
    public async Task GetAllFuels_WithData_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var f1 = new FuelBuilder().Build();
        var f2 = new FuelBuilder().Build();
        await context.AddRangeAsync(f1, f2);
        await context.SaveChangesAsync();

        var handler = new GetAllFuelsQueryHandler(mapper, repository);

        var result = await handler.Handle(new GetAllFuelsQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // === GetAllTransmissions ===

    [Fact]
    public async Task GetAllTransmissions_WithData_ReturnsAll()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var t1 = new TransmissionBuilder().Build();
        var t2 = new TransmissionBuilder().Build();
        await context.AddRangeAsync(t1, t2);
        await context.SaveChangesAsync();

        var handler = new GetAllTransmissionsQueryHandler(mapper, repository);

        var result = await handler.Handle(new GetAllTransmissionsQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
