namespace Volue.ForecastService.Contracts.Models;

/// <summary>
/// Represents the aggregated power generation position for a company across all its plants.
/// </summary>
public record CompanyPosition
{
    /// <summary>
    /// The company identifier.
    /// </summary>
    public Guid CompanyId { get; init; }

    /// <summary>
    /// The company name.
    /// </summary>
    public string CompanyName { get; init; } = string.Empty;

    /// <summary>
    /// Hourly positions aggregated from all power plants.
    /// </summary>
    public IReadOnlyList<HourlyPosition> Positions { get; init; } = Array.Empty<HourlyPosition>();

    /// <summary>
    /// Total aggregated MWh across all hours and plants.
    /// </summary>
    public decimal TotalMwh => Positions.Sum(p => p.TotalMwh);
}

/// <summary>
/// Represents the aggregated power generation for a specific hour.
/// </summary>
public record HourlyPosition
{
    /// <summary>
    /// The hour in UTC.
    /// </summary>
    public DateTime HourUtc { get; init; }

    /// <summary>
    /// Total MWh for this hour across all plants.
    /// </summary>
    public decimal TotalMwh { get; init; }

    /// <summary>
    /// Number of plants contributing to this hour.
    /// </summary>
    public int PlantCount { get; init; }
}
