namespace Automotive.Marketplace.Tests.FeatureTests.AuthFeatureTests;

using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
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

public class AuthenticateAccountTests : IClassFixture<TestFixture>
{
    private readonly TestFixture fixture;

    public AuthenticateAccountTests(TestFixture fixture)
    {
        this.fixture = fixture;
        this.fixture.MockUnitOfWork.Reset();
        this.fixture.MockMapper.Reset();
    }
}
