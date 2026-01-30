using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volue.ForecastService.Contracts.Services;
using Volue.ForecastService.Services.Messaging;

namespace Volue.ForecastService.Services;

/// <summary>
/// Extension methods for registering service layer dependencies.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all service implementations with the dependency injection container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IForecastService, ForecastService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IPowerPlantService, PowerPlantService>();

        // Register event publisher based on configuration
        var rabbitMqEnabled = bool.Parse(configuration["RabbitMQ:Enabled"] ?? "false");
        if (rabbitMqEnabled)
        {
            services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();
        }
        else
        {
            services.AddSingleton<IEventPublisher, NullEventPublisher>();
        }

        return services;
    }
}
