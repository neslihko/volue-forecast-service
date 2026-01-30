using Microsoft.AspNetCore.Mvc;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Api.Controllers;

/// <summary>
/// API controller for power plant information.
/// </summary>
[ApiController]
[Route("api/power-plants")]
[Produces("application/json")]
public class PowerPlantsController : ControllerBase
{
    private readonly IPowerPlantService _powerPlantService;
    private readonly ILogger<PowerPlantsController> _logger;

    public PowerPlantsController(
        IPowerPlantService powerPlantService,
        ILogger<PowerPlantsController> logger)
    {
        _powerPlantService = powerPlantService ?? throw new ArgumentNullException(nameof(powerPlantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all active power plants.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Power plants retrieved successfully</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<PowerPlantInfo>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllActivePlants(CancellationToken cancellationToken)
    {
        var result = await _powerPlantService.GetAllActivePlantsAsync(cancellationToken);

        return Ok(ApiResponse<IEnumerable<PowerPlantInfo>>.SuccessResult(result.Value));
    }

    /// <summary>
    /// Retrieves a specific power plant by ID.
    /// </summary>
    /// <param name="plantId">The power plant identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Power plant retrieved successfully</response>
    /// <response code="404">Power plant not found</response>
    [HttpGet("{plantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PowerPlantInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PowerPlantInfo>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlantById(Guid plantId, CancellationToken cancellationToken)
    {
        var result = await _powerPlantService.GetPlantByIdAsync(plantId, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(ApiResponse<PowerPlantInfo>.FailureResult(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<PowerPlantInfo>.SuccessResult(result.Value));
    }
}
