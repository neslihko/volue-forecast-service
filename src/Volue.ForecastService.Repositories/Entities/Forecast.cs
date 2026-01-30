namespace Volue.ForecastService.Repositories.Entities;

/// <summary>
/// Represents a power generation forecast for a specific plant and hour.
/// </summary>
public class Forecast
{
    public Guid Id { get; set; }
    public Guid PlantId { get; set; }
    public DateTime HourUtc { get; set; }
    public decimal Mwh { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public PowerPlant Plant { get; set; } = null!;
}
