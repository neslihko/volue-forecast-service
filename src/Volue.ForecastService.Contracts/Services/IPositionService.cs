using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Services;

/// <summary>
/// Service for retrieving company positions (aggregated forecasts).
/// </summary>
public interface IPositionService
{
    /// <summary>
    /// Retrieves the aggregated power generation position for a company.
    /// </summary>
    Task<Result<CompanyPosition>> GetCompanyPositionAsync(
        Guid companyId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
