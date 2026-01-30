namespace Volue.ForecastService.Contracts.Models;

/// <summary>
/// Represents a single forecast data point for a specific hour.
/// </summary>
public record ForecastPoint
{
    /// <summary>
    /// The hour for which the forecast is made (must be hour-aligned UTC).
    /// </summary>
    public DateTime HourUtc { get; init; }

    /// <summary>
    /// The forecasted power generation in megawatt-hours (MWh).
    /// </summary>
    public decimal Mwh { get; init; }
}
