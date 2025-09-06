namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface IDevelopmentSeeder
{
    public Task SeedAsync(CancellationToken cancellationToken);
}