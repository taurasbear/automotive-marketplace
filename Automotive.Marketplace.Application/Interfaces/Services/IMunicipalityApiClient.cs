namespace Automotive.Marketplace.Application.Interfaces.Services;

public record MunicipalityDto(Guid Id, string Name);

public interface IMunicipalityApiClient
{
    Task<IEnumerable<MunicipalityDto>> FetchMunicipalitiesAsync(CancellationToken cancellationToken = default);
}
