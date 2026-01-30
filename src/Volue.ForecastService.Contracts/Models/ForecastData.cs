namespace Volue.ForecastService.Contracts.Models;

/// <summary>
/// Represents forecast data for a power plant.
/// </summary>
public record ForecastData
{
    public Guid Id { get; init; }
    public Guid PlantId { get; init; }
    public DateTime HourUtc { get; init; }
    public decimal Mwh { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
