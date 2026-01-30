using Microsoft.Extensions.Logging;
using Volue.ForecastService.Contracts.Events;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Services.Messaging;

/// <summary>
/// Null object pattern implementation for event publishing.
/// Used when RabbitMQ is disabled or unavailable.
/// </summary>
public class NullEventPublisher : IEventPublisher
{
    private readonly ILogger<NullEventPublisher> _logger;

    public NullEventPublisher(ILogger<NullEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task PublishPositionChangedAsync(
        PositionChangedEvent @event,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "NullEventPublisher: Event publishing disabled. Event {EventId} not published",
            @event.EventId);

        return Task.CompletedTask;
    }
}
