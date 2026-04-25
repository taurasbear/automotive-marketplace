namespace Automotive.Marketplace.Application.Interfaces.Services;

public record VpicMakeDto(int VpicId, string VpicName);
public record VpicModelDto(int VpicId, string VpicName, int MakeVpicId);

public interface IVehicleDataApiClient
{
    Task<IEnumerable<VpicMakeDto>> FetchCarMakesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VpicModelDto>> FetchModelsForMakeAsync(int vpicMakeId, CancellationToken cancellationToken = default);
}
