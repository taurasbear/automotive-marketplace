namespace Automotive.Marketplace.Infrastructure.Interfaces;

public interface IVehicleDataInitializer
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
