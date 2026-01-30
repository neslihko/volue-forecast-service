using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.Models;

namespace Volue.ForecastService.Contracts.Services;

/// <summary>
/// Service for retrieving power plant information.
/// </summary>
public interface IPowerPlantService
{
    /// <summary>
    /// Retrieves all active power plants.
    /// </summary>
    Task<Result<IEnumerable<PowerPlantInfo>>> GetAllActivePlantsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific power plant by ID.
    /// </summary>
    Task<Result<PowerPlantInfo>> GetPlantByIdAsync(
        Guid plantId,
        CancellationToken cancellationToken = default);
}
