using Microsoft.Extensions.Logging;
using Volue.ForecastService.Contracts.Common;
using Volue.ForecastService.Contracts.DTOs;
using Volue.ForecastService.Contracts.Events;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Services;

/// <summary>
/// Service implementation for managing power generation forecasts with validation.
/// </summary>
public class ForecastService : IForecastService
{
    private readonly IForecastWriteRepository _writeRepository;
    private readonly IForecastReadRepository _readRepository;
    private readonly IPowerPlantRepository _powerPlantRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ForecastService> _logger;

    private const int MaxQueryRangeDays = 7;

    public ForecastService(
        IForecastWriteRepository writeRepository,
        IForecastReadRepository readRepository,
        IPowerPlantRepository powerPlantRepository,
        IEventPublisher eventPublisher,
        ILogger<ForecastService> logger)
    {
        _writeRepository = writeRepository ?? throw new ArgumentNullException(nameof(writeRepository));
        _readRepository = readRepository ?? throw new ArgumentNullException(nameof(readRepository));
        _powerPlantRepository = powerPlantRepository ?? throw new ArgumentNullException(nameof(powerPlantRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<CreateOrUpdateForecastResponse>> CreateOrUpdateForecastAsync(
        Guid plantId,
        CreateOrUpdateForecastRequest request,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate plant exists and get plant info for event publishing
        var plant = await _powerPlantRepository.GetPlantByIdAsync(plantId, cancellationToken);
        if (plant == null || !plant.IsActive)
        {
            _logger.LogWarning("Plant {PlantId} not found or inactive", plantId);
            return Result.Failure<CreateOrUpdateForecastResponse>(DomainErrors.Forecast.PlantNotFound(plantId));
        }

        // Validate request has data
        if (request.Forecasts.Count == 0)
        {
            return Result.Failure<CreateOrUpdateForecastResponse>(DomainErrors.Forecast.NoDataPoints);
        }

        // Validate all forecast points
        foreach (var point in request.Forecasts)
        {
            var validationResult = ValidateForecastPoint(point);
            if (validationResult.IsFailure)
            {
                return Result.Failure<CreateOrUpdateForecastResponse>(validationResult.Error);
            }
        }

        // Perform UPSERT
        var upsertResult = await _writeRepository.UpsertForecastsAsync(
            plantId,
            request.Forecasts,
            cancellationToken);

        _logger.LogInformation(
            "Forecast upserted for Plant {PlantId}: {Inserted} inserted, {Updated} updated, {Unchanged} unchanged",
            plantId, upsertResult.InsertedCount, upsertResult.UpdatedCount, upsertResult.UnchangedCount);

        // Publish event if changes occurred
        if (upsertResult.HasChanges)
        {
            var timeRange = GetTimeRange(request.Forecasts);
            var positionEvent = new PositionChangedEvent
            {
                CompanyId = plant.CompanyId,
                PlantId = plantId,
                FromHourUtc = timeRange.From,
                ToHourUtc = timeRange.To,
                InsertedCount = upsertResult.InsertedCount,
                UpdatedCount = upsertResult.UpdatedCount,
                CorrelationId = correlationId ?? string.Empty
            };

            // Fire-and-forget: don't block on event publishing
            _ = Task.Run(async () =>
            {
                try
                {
                    await _eventPublisher.PublishPositionChangedAsync(positionEvent, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish position changed event for Plant {PlantId}", plantId);
                }
            }, cancellationToken);
        }

        var response = new CreateOrUpdateForecastResponse
        {
            PlantId = plantId,
            InsertedCount = upsertResult.InsertedCount,
            UpdatedCount = upsertResult.UpdatedCount,
            UnchangedCount = upsertResult.UnchangedCount,
            TotalProcessed = upsertResult.TotalProcessed,
            HasChanges = upsertResult.HasChanges
        };

        return Result.Success(response);
    }

    public async Task<Result<GetForecastResponse>> GetForecastAsync(
        Guid plantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        // Validate plant exists
        var plantExists = await _readRepository.PlantExistsAsync(plantId, cancellationToken);
        if (!plantExists)
        {
            _logger.LogWarning("Plant {PlantId} not found or inactive", plantId);
            return Result.Failure<GetForecastResponse>(DomainErrors.Forecast.PlantNotFound(plantId));
        }

        // Validate time range
        var validationResult = ValidateTimeRange(fromUtc, toUtc);
        if (validationResult.IsFailure)
        {
            return Result.Failure<GetForecastResponse>(validationResult.Error);
        }

        // Retrieve forecasts
        var forecasts = await _readRepository.GetForecastsAsync(plantId, fromUtc, toUtc, cancellationToken);

        var response = new GetForecastResponse
        {
            PlantId = plantId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Forecasts = forecasts.ToList()
        };

        _logger.LogInformation(
            "Retrieved {Count} forecasts for Plant {PlantId} from {From} to {To}",
            response.Forecasts.Count, plantId, fromUtc, toUtc);

        return Result.Success(response);
    }

    private static Result ValidateForecastPoint(ForecastPoint point)
    {
        // Validate UTC
        if (point.HourUtc.Kind != DateTimeKind.Utc)
        {
            return Result.Failure(DomainErrors.Forecast.InvalidTimezone);
        }

        // Validate hour-aligned (minutes and seconds must be 0)
        if (point.HourUtc.Minute != 0 || point.HourUtc.Second != 0 || point.HourUtc.Millisecond != 0)
        {
            return Result.Failure(DomainErrors.Forecast.InvalidHourAlignment);
        }

        // Validate non-negative MWh
        if (point.Mwh < 0)
        {
            return Result.Failure(DomainErrors.Forecast.NegativeMwh);
        }

        return Result.Success();
    }

    private static Result ValidateTimeRange(DateTime fromUtc, DateTime toUtc)
    {
        // Validate UTC
        if (fromUtc.Kind != DateTimeKind.Utc || toUtc.Kind != DateTimeKind.Utc)
        {
            return Result.Failure(DomainErrors.Forecast.InvalidTimezone);
        }

        // Validate hour-aligned
        if (fromUtc.Minute != 0 || fromUtc.Second != 0 || fromUtc.Millisecond != 0 ||
            toUtc.Minute != 0 || toUtc.Second != 0 || toUtc.Millisecond != 0)
        {
            return Result.Failure(DomainErrors.Forecast.InvalidHourAlignment);
        }

        // Validate from < to
        if (fromUtc >= toUtc)
        {
            return Result.Failure(DomainErrors.Forecast.InvalidTimeRange);
        }

        // Validate max range
        if ((toUtc - fromUtc).TotalDays > MaxQueryRangeDays)
        {
            return Result.Failure(DomainErrors.Forecast.TimeRangeTooLarge(MaxQueryRangeDays));
        }

        return Result.Success();
    }

    private static (DateTime From, DateTime To) GetTimeRange(IReadOnlyList<ForecastPoint> forecasts)
    {
        var sortedHours = forecasts.Select(f => f.HourUtc).OrderBy(h => h).ToList();
        return (sortedHours.First(), sortedHours.Last());
    }
}
