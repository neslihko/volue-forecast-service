using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.DTOs;

/// <summary>
/// Request to create or update forecasts for a power plant.
/// </summary>
public record CreateOrUpdateForecastRequest
{
    /// <summary>
    /// Collection of forecast data points.
    /// </summary>
    public IReadOnlyList<ForecastPoint> Forecasts { get; init; } = Array.Empty<ForecastPoint>();
}

/// <summary>
/// Response from creating or updating forecasts.
/// </summary>
public record CreateOrUpdateForecastResponse
{
    public Guid PlantId { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int UnchangedCount { get; init; }
    public int TotalProcessed { get; init; }
    public bool HasChanges { get; init; }
}

/// <summary>
/// Response containing forecasts for a power plant.
/// </summary>
public record GetForecastResponse
{
    public Guid PlantId { get; init; }
    public DateTime FromUtc { get; init; }
    public DateTime ToUtc { get; init; }
    public IReadOnlyList<ForecastData> Forecasts { get; init; } = Array.Empty<ForecastData>();
}
