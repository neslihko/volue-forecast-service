using Microsoft.EntityFrameworkCore;
using Volue.ForecastService.Contracts.Models;
using Volue.ForecastService.Contracts.Repositories;

namespace Volue.ForecastService.Repositories;

/// <summary>
/// Repository implementation for reading forecast data.
/// </summary>
public class ForecastReadRepository : IForecastReadRepository
{
    private readonly ForecastDbContext _context;

    public ForecastReadRepository(ForecastDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<ForecastData>> GetForecastsAsync(
        Guid plantId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var forecasts = await _context.Forecasts
            .AsNoTracking()
            .Where(f => f.PlantId == plantId
                && f.HourUtc >= fromUtc
                && f.HourUtc < toUtc)
            .OrderBy(f => f.HourUtc)
            .Select(f => new ForecastData
            {
                Id = f.Id,
                PlantId = f.PlantId,
                HourUtc = f.HourUtc,
                Mwh = f.Mwh,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return forecasts;
    }

    public async Task<bool> PlantExistsAsync(
        Guid plantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.PowerPlants
            .AsNoTracking()
            .AnyAsync(p => p.Id == plantId && p.IsActive, cancellationToken);
    }
}
