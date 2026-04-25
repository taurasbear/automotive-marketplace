namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface IMunicipalityInitializer
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
