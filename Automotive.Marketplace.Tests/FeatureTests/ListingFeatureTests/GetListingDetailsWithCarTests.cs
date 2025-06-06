namespace Automotive.Marketplace.Tests.FeatureTests.ListingFeatureTests;

using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;

public class TestFixture : IDisposable
{
    public Mock<IUnitOfWork> MockUnitOfWork { get; }
    public Mock<IMapper> MockMapper { get; }

    public TestFixture()
    {
        MockUnitOfWork = new Mock<IUnitOfWork>();
        MockMapper = new Mock<IMapper>();
    }

    public void Dispose()
    { }
}

public class GetListingDetailsWithCarTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;

    public GetListingDetailsWithCarTests(TestFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.MockUnitOfWork.Reset();
        this.fixture.MockMapper.Reset();
    }

    [Fact]
    public async Task Handle_ListingDetailsWithCarExists_GetListingDetailsWithCar()
    {
        // Arrange
        var listings = new List<Listing> { new Listing() };
        var mappedResponse = new GetListingDetailsWithCarResponse
        {
            ListingDetailsWithCar = new List<GetListingDetailsWithCarResponse.GetListingWithCarResponse>
            {
                new GetListingDetailsWithCarResponse.GetListingWithCarResponse
                {
                    Year = "2015",
                    Make = "Tesla",
                    Model = "Model S",
                    Mileage = 50000,
                    Power = 150,
                    EngineSize = 2000,
                    Used = true,
                    Price = 30000,
                    City = "San Francisco",
                    Description = "Excellent condition",
                    FuelType = "Electric",
                    Transmission = "Automatic"
                }
            }
        };

        this.fixture.MockUnitOfWork
            .Setup(uow => uow.ListingRepository.GetListingDetailsWithCarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(listings);

        this.fixture.MockMapper
            .Setup(mapper => mapper.Map<GetListingDetailsWithCarResponse>(listings))
            .Returns(mappedResponse);

        var handler = new GetListingDetailsWithCarHandler(this.fixture.MockMapper.Object, this.fixture.MockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(new GetListingDetailsWithCarRequest(), CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeEquivalentTo(mappedResponse);

            this.fixture.MockUnitOfWork
                .Verify(uow => uow.ListingRepository.GetListingDetailsWithCarAsync(It.IsAny<CancellationToken>()), Times.Once);
            this.fixture.MockMapper.Verify(mapper => mapper.Map<GetListingDetailsWithCarResponse>(listings), Times.Once);
        }
    }

    [Fact]
    public async Task Handle_RepositoryReturnsEmptyList_ReturnsEmptyMappedResponse()
    {
        // Arrange

        var listings = new List<Listing>();
        var mappedResponse = new GetListingDetailsWithCarResponse
        {
            ListingDetailsWithCar = new List<GetListingDetailsWithCarResponse.GetListingWithCarResponse>()
        };

        this.fixture.MockUnitOfWork
            .Setup(uow => uow.ListingRepository.GetListingDetailsWithCarAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(listings);

        this.fixture.MockMapper
            .Setup(mapper => mapper.Map<GetListingDetailsWithCarResponse>(listings))
            .Returns(mappedResponse);

        var handler = new GetListingDetailsWithCarHandler(this.fixture.MockMapper.Object, this.fixture.MockUnitOfWork.Object);

        // Act
        var result = await handler.Handle(new GetListingDetailsWithCarRequest(), CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            result.Should().BeEquivalentTo(mappedResponse);

            this.fixture.MockUnitOfWork
                .Verify(uow => uow.ListingRepository.GetListingDetailsWithCarAsync(It.IsAny<CancellationToken>()), Times.Once);
            this.fixture.MockMapper.Verify(mapper => mapper.Map<GetListingDetailsWithCarResponse>(listings), Times.Once);
        }
    }
}
