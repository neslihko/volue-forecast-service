using Microsoft.Extensions.DependencyInjection;
using Volue.ForecastService.Contracts.Repositories;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Extension methods for registering repository services.
/// </summary>
public static class RepositoryServiceExtensions
{
    /// <summary>
    /// Registers all repository implementations with the dependency injection container.
    /// </summary>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IForecastWriteRepository, ForecastWriteRepository>();
        services.AddScoped<IForecastReadRepository, ForecastReadRepository>();
        services.AddScoped<IPositionReadRepository, PositionReadRepository>();
        services.AddScoped<IPowerPlantRepository, PowerPlantRepository>();

        return services;
    }
}
