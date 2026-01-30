using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Repositories;

/// <summary>
/// Repository for reading company position data (aggregated forecasts).
/// </summary>
public interface IPositionReadRepository
{
    /// <summary>
    /// Retrieves the aggregated power generation position for a company across all its plants.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <param name="fromUtc">Start of the time range (inclusive, UTC).</param>
    /// <param name="toUtc">End of the time range (exclusive, UTC).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The company position with hourly aggregations.</returns>
    Task<CompanyPosition?> GetCompanyPositionAsync(
        Guid companyId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a company exists and is active.
    /// </summary>
    /// <param name="companyId">The company identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the company exists and is active, false otherwise.</returns>
    Task<bool> CompanyExistsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);
}
