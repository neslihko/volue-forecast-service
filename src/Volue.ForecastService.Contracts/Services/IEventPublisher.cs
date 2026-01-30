using Volue.ForecastService.Contracts.Events;

namespace Volue.ForecastService.Contracts.Services;

/// <summary>
/// Service for publishing domain events to a message broker.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes a position changed event asynchronously (fire-and-forget).
    /// </summary>
    /// <param name="event">The event to publish.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task PublishPositionChangedAsync(PositionChangedEvent @event, CancellationToken cancellationToken = default);
}
