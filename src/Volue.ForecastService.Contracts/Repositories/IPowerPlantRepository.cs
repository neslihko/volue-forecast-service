using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Repositories;

/// <summary>
/// Repository for reading power plant information.
/// </summary>
public interface IPowerPlantRepository
{
    /// <summary>
    /// Retrieves all active power plants.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of power plant information.</returns>
    Task<IEnumerable<PowerPlantInfo>> GetAllActivePlantsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific power plant by ID.
    /// </summary>
    /// <param name="plantId">The power plant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Power plant information or null if not found.</returns>
    Task<PowerPlantInfo?> GetPlantByIdAsync(
        Guid plantId,
        CancellationToken cancellationToken = default);
}
