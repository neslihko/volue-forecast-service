using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Volue.ForecastService.Contracts.Events;
using Volue.ForecastService.Contracts.Services;

namespace Volue.ForecastService.Services.Messaging;

/// <summary>
/// RabbitMQ implementation of event publisher.
/// Publishes events with graceful degradation if RabbitMQ is unavailable.
/// </summary>
public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _isEnabled;
    private readonly object _lock = new();

    public RabbitMqEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _exchangeName = configuration["RabbitMQ:ExchangeName"] ?? "forecast.events";
        _routingKey = configuration["RabbitMQ:RoutingKey"] ?? "position.changed";
        _isEnabled = bool.Parse(configuration["RabbitMQ:Enabled"] ?? "false");

        if (_isEnabled)
        {
            TryInitializeConnection(configuration);
        }
        else
        {
            _logger.LogInformation("RabbitMQ event publishing is disabled");
        }
    }

    private void TryInitializeConnection(IConfiguration configuration)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "localhost",
                Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchange (idempotent)
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation(
                "RabbitMQ connection established. Exchange: {Exchange}, RoutingKey: {RoutingKey}",
                _exchangeName, _routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to initialize RabbitMQ connection. Event publishing will be disabled. Error: {Message}",
                ex.Message);
            _isEnabled = false;
            DisposeResources();
        }
    }

    public async Task PublishPositionChangedAsync(
        PositionChangedEvent @event,
        CancellationToken cancellationToken = default)
    {
        if (!_isEnabled || _channel == null || !_channel.IsOpen)
        {
            _logger.LogDebug("Event publishing skipped (RabbitMQ disabled or unavailable): {EventId}", @event.EventId);
            return;
        }

        try
        {
            // Serialize event to JSON
            var message = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var body = Encoding.UTF8.GetBytes(message);

            // Publish properties
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = @event.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.Headers = new Dictionary<string, object>
            {
                ["correlation-id"] = @event.CorrelationId,
                ["event-type"] = nameof(PositionChangedEvent),
                ["company-id"] = @event.CompanyId.ToString(),
                ["plant-id"] = @event.PlantId.ToString()
            };

            lock (_lock)
            {
                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: _routingKey,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogInformation(
                "Published PositionChangedEvent: EventId={EventId}, CompanyId={CompanyId}, PlantId={PlantId}, CorrelationId={CorrelationId}",
                @event.EventId, @event.CompanyId, @event.PlantId, @event.CorrelationId);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            // Fire-and-forget: log but don't throw
            _logger.LogError(ex,
                "Failed to publish PositionChangedEvent: {EventId}. Error: {Message}",
                @event.EventId, ex.Message);
        }
    }

    private void DisposeResources()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing RabbitMQ resources: {Message}", ex.Message);
        }
    }

    public void Dispose()
    {
        DisposeResources();
        GC.SuppressFinalize(this);
    }
}
