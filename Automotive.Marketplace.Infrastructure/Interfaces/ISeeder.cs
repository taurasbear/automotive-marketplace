namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface ISeeder
{
    public Task SeedAsync(CancellationToken cancellationToken);
}
