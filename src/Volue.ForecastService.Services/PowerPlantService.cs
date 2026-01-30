using Microsoft.Extensions.Logging;
using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Services;

/// <summary>
/// Service implementation for power plant operations.
/// </summary>
public class PowerPlantService : IPowerPlantService
{
    private readonly IPowerPlantRepository _repository;
    private readonly ILogger<PowerPlantService> _logger;

    public PowerPlantService(
        IPowerPlantRepository repository,
        ILogger<PowerPlantService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<PowerPlantInfo>>> GetAllActivePlantsAsync(
        CancellationToken cancellationToken = default)
    {
        var plants = await _repository.GetAllActivePlantsAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} active power plants", plants.Count());

        return Result.Success(plants);
    }

    public async Task<Result<PowerPlantInfo>> GetPlantByIdAsync(
        Guid plantId,
        CancellationToken cancellationToken = default)
    {
        var plant = await _repository.GetPlantByIdAsync(plantId, cancellationToken);

        if (plant == null)
        {
            _logger.LogWarning("Power plant {PlantId} not found", plantId);
            return Result.Failure<PowerPlantInfo>(DomainErrors.PowerPlant.NotFound(plantId));
        }

        return Result.Success(plant);
    }
}
