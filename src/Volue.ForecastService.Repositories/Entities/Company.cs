namespace Volue.ForecastService.Repositories.Entities;

/// <summary>
/// Represents an energy trading company that owns power plants.
/// </summary>
public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation property
    public ICollection<PowerPlant> PowerPlants { get; set; } = new List<PowerPlant>();
}
