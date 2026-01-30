using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Repositories;

/// <summary>
/// Repository for reading forecast data.
/// </summary>
public interface IForecastReadRepository
{
    /// <summary>
    /// Retrieves forecasts for a specific power plant within a time range.
    /// </summary>
    /// <param name="plantId">The power plant identifier.</param>
    /// <param name="fromUtc">Start of the time range (inclusive, UTC).</param>
    /// <param name="toUtc">End of the time range (exclusive, UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of forecast data ordered by hour.</returns>
    Task<IEnumerable<ForecastData>> GetForecastsAsync(
        Guid plantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a power plant exists and is active.
    /// </summary>
    /// <param name="plantId">The power plant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the plant exists and is active, false otherwise.</returns>
    Task<bool> PlantExistsAsync(
        Guid plantId,
        CancellationToken cancellationToken = default);
}
