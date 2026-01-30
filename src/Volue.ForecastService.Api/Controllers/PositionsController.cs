using Microsoft.AspNetCore.Mvc;
using Volue.ForecastService.Api.Models;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Api.Controllers;

/// <summary>
/// API controller for retrieving company positions (aggregated forecasts).
/// </summary>
[ApiController]
[Route("api/company")]
[Produces("application/json")]
public class PositionsController : ControllerBase
{
    private readonly IPositionService _positionService;
    private readonly ILogger<PositionsController> _logger;

    public PositionsController(
        IPositionService positionService,
        ILogger<PositionsController> logger)
    {
        _positionService = positionService ?? throw new ArgumentNullException(nameof(positionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves the aggregated power generation position for a company across all its plants.
    /// </summary>
    /// <param name="companyId">The company identifier</param>
    /// <param name="from">Start of time range (ISO 8601 UTC format)</param>
    /// <param name="to">End of time range (ISO 8601 UTC format)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Company position retrieved successfully</response>
    /// <response code="400">Invalid request (validation error)</response>
    /// <response code="404">Company not found</response>
    [HttpGet("{companyId:guid}/position")]
    [ProducesResponseType(typeof(ApiResponse<CompanyPosition>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CompanyPosition>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CompanyPosition>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCompanyPosition(
        Guid companyId,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        var result = await _positionService.GetCompanyPositionAsync(companyId, from, to, cancellationToken);

        if (result.IsFailure)
        {
            var statusCode = result.Error.Code.Contains("NotFound") ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
            return StatusCode(statusCode, ApiResponse<CompanyPosition>.FailureResult(result.Error.Code, result.Error.Message));
        }

        return Ok(ApiResponse<CompanyPosition>.SuccessResult(result.Value));
    }
}
