using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Repositories;

/// <summary>
/// Repository for writing forecast data with UPSERT support.
/// </summary>
public interface IForecastWriteRepository
{
    /// <summary>
    /// Performs a bulk UPSERT operation for forecast data.
    /// If a forecast already exists for a given (plantId, hourUtc), it will be updated.
    /// Otherwise, a new forecast will be inserted.
    /// </summary>
    /// <param name="plantId">The power plant identifier.</param>
    /// <param name="points">The forecast data points to upsert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the UPSERT operation containing counts of inserted/updated/unchanged records.</returns>
    Task<UpsertResult> UpsertForecastsAsync(
        Guid plantId,
        IEnumerable<ForecastPoint> points,
        CancellationToken cancellationToken = default);
}
