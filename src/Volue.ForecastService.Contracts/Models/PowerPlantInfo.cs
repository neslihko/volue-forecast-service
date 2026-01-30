namespace Volue.ForecastService.Contracts.Models;

/// <summary>
/// Represents basic information about a power plant.
/// </summary>
public record PowerPlantInfo
{
    public Guid Id { get; init; }
    public Guid CompanyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
    public decimal CapacityMwh { get; init; }
    public bool IsActive { get; init; }
}
