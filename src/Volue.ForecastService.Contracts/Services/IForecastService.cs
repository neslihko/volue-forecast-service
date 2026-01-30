using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.DTOs;
using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Services;

/// <summary>
/// Service for managing power generation forecasts.
/// </summary>
public interface IForecastService
{
    /// <summary>
    /// Creates or updates forecasts for a power plant.
    /// Validates input and performs UPSERT operation.
    /// </summary>
    Task<Result<CreateOrUpdateForecastResponse>> CreateOrUpdateForecastAsync(
        Guid plantId,
        CreateOrUpdateForecastRequest request,
        string? correlationId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves forecasts for a power plant within a time range.
    /// </summary>
    Task<Result<GetForecastResponse>> GetForecastAsync(
        Guid plantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
