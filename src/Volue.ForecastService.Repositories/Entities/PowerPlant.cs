namespace Volue.ForecastService.Repositories.Entities;

/// <summary>
/// Represents a power generation plant (wind, solar, hydro, etc.).
/// </summary>
public class PowerPlant
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public decimal CapacityMwh { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();
}
