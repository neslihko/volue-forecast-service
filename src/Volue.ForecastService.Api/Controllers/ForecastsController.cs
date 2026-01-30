using Microsoft.AspNetCore.Mvc;
using Volue.ForecastService.Api.Middleware;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.DTOs;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Api.Controllers;

/// <summary>
/// API controller for managing power generation forecasts.
/// </summary>
[ApiController]
[Route("api/forecasts")]
[Produces("application/json")]
public class ForecastsController : ControllerBase
{
    private readonly IForecastService _forecastService;
    private readonly ILogger<ForecastsController> _logger;

    public ForecastsController(
        IForecastService forecastService,
        ILogger<ForecastsController> logger)
    {
        _forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates or updates forecasts for a specific power plant.
    /// </summary>
    /// <param name="plantId">The power plant identifier</param>
    /// <param name="request">The forecast data points</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Forecasts successfully created/updated</response>
    /// <response code="400">Invalid request (validation error)</response>
    /// <response code="404">Power plant not found</response>
    [HttpPut("{plantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CreateOrUpdateForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateOrUpdateForecastResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateOrUpdateForecastResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrUpdateForecast(
        Guid plantId,
        [FromBody] CreateOrUpdateForecastRequest request,
        CancellationToken cancellationToken)
    {
        var correlationId = HttpContext.GetCorrelationId();
        var result = await _forecastService.CreateOrUpdateForecastAsync(plantId, request, correlationId, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return StatusCode(statusCode, ApiResponse<CreateOrUpdateForecastResponse>.FailureResult(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<CreateOrUpdateForecastResponse>.SuccessResult(result.Value));
    }

    /// <summary>
    /// Retrieves forecasts for a specific power plant within a time range.
    /// </summary>
    /// <param name="plantId">The power plant identifier</param>
    /// <param name="from">Start of time range (ISO 8601 UTC format)</param>
    /// <param name="to">End of time range (ISO 8601 UTC format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Forecasts retrieved successfully</response>
    /// <response code="400">Invalid request (validation error)</response>
    /// <response code="404">Power plant not found</response>
    [HttpGet("{plantId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GetForecastResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<GetForecastResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<GetForecastResponse>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForecast(
        Guid plantId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _forecastService.GetForecastAsync(plantId, from, to, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return StatusCode(statusCode, ApiResponse<GetForecastResponse>.FailureResult(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<GetForecastResponse>.SuccessResult(result.Value));
    }
}
