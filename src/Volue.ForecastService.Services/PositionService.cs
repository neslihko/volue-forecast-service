using Microsoft.Extensions.Logging;
using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Services;

/// <summary>
/// Service implementation for retrieving company positions with validation.
/// </summary>
public class PositionService : IPositionService
{
    private readonly IPositionReadRepository _repository;
    private readonly ILogger<PositionService> _logger;

    private const int MaxQueryRangeDays = 7;

    public PositionService(
        IPositionReadRepository repository,
        ILogger<PositionService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CompanyPosition>> GetCompanyPositionAsync(
        Guid companyId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        // Validate company exists
        var companyExists = await _repository.CompanyExistsAsync(companyId, cancellationToken);
        if (!companyExists)
        {
            _logger.LogWarning("Company {CompanyId} not found or inactive", companyId);
            return Result.Failure<CompanyPosition>(DomainErrors.Position.CompanyNotFound(companyId));
        }

        // Validate time range
        var validationResult = ValidateTimeRange(fromUtc, toUtc);
        if (validationResult.IsFailure)
        {
            return Result.Failure<CompanyPosition>(validationResult.Error);
        }

        // Retrieve position
        var position = await _repository.GetCompanyPositionAsync(companyId, fromUtc, toUtc, cancellationToken);

        if (position == null)
        {
            _logger.LogWarning("Company {CompanyId} not found", companyId);
            return Result.Failure<CompanyPosition>(DomainErrors.Position.CompanyNotFound(companyId));
        }

        _logger.LogInformation(
            "Retrieved position for Company {CompanyId} ({CompanyName}): {HourCount} hours, {TotalMwh} MWh total",
            companyId, position.CompanyName, position.Positions.Count, position.TotalMwh);

        return Result.Success(position);
    }

    private static Result ValidateTimeRange(DateTime fromUtc, DateTime toUtc)
    {
        // Validate UTC
        if (fromUtc.Kind != DateTimeKind.Utc || toUtc.Kind != DateTimeKind.Utc)
        {
            return Result.Failure(DomainErrors.Position.InvalidTimezone);
        }

        // Validate hour-aligned
        if (fromUtc.Minute != 0 || fromUtc.Second != 0 || fromUtc.Millisecond != 0 ||
            toUtc.Minute != 0 || toUtc.Second != 0 || toUtc.Millisecond != 0)
        {
            return Result.Failure(DomainErrors.Position.InvalidHourAlignment);
        }

        // Validate from < to
        if (fromUtc >= toUtc)
        {
            return Result.Failure(DomainErrors.Position.InvalidTimeRange);
        }

        // Validate max range
        if ((toUtc - fromUtc).TotalDays > MaxQueryRangeDays)
        {
            return Result.Failure(DomainErrors.Position.TimeRangeTooLarge(MaxQueryRangeDays));
        }

        return Result.Success();
    }
}
