namespace Volue.ForecastService.Contracts.Events;

/// <summary>
/// Event published when a company's position changes due to forecast updates.
/// </summary>
public record PositionChangedEvent
{
    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The company whose position changed.
    /// </summary>
    public Guid CompanyId { get; init; }

    /// <summary>
    /// The power plant that caused the position change.
    /// </summary>
    public Guid PlantId { get; init; }

    /// <summary>
    /// Start of the affected time range.
    /// </summary>
    public DateTime FromHourUtc { get; init; }

    /// <summary>
    /// End of the affected time range.
    /// </summary>
    public DateTime ToHourUtc { get; init; }

    /// <summary>
    /// When the event occurred (UTC).
    /// </summary>
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing this operation across services.
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;

    /// <summary>
    /// Number of forecasts inserted.
    /// </summary>
    public int InsertedCount { get; init; }

    /// <summary>
    /// Number of forecasts updated.
    /// </summary>
    public int UpdatedCount { get; init; }
}
